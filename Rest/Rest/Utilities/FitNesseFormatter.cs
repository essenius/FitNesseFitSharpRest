// Copyright 2015-2020 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
#if NET48
using System.Web;
#else
using Microsoft.Net.Http.Headers;
#endif

namespace Rest.Utilities
{
    internal static class FitNesseFormatter
    {
        #region Private Fields
        /// <summary>
        /// Valid cookie options.
        /// </summary>
        private static HashSet<string> _cookieOptions = new HashSet<string>(){"expires", "max-age", "domain", "path", "secure", "httponly", "samesite"};
        #endregion

        #region Public Methods
        /// <param name="cookies">a cookie collection (can be null)</param>
        /// <returns>string representations of the cookies, each cookie on a separate line. Returns null if cookies is null</returns>
        public static string CookieList(CookieCollection cookies)
        {
            if (cookies == null) return null;
            var result = new List<string>();
            foreach (Cookie cookie in cookies)
            {
                var httpOnly = cookie.HttpOnly ? "HttpOnly; " : string.Empty;
                var expires = cookie.Expires.Ticks == 0 ? string.Empty : $"Expires={cookie.Expires.ToUniversalTime():R}; ";
                var path = string.IsNullOrEmpty(cookie.Path) ? string.Empty : $"Path={cookie.Path}; ";
                var secure = cookie.Secure ? "Secure; " : string.Empty;
                result.Add($"{cookie.Name}={cookie.Value}; {expires}{path}Domain={cookie.Domain}; {httpOnly}{secure}".Trim().TrimEnd(';'));
            }
            return string.Join("\n", result);
        }

        /// <param name="headers">a name value collection representing HTTP headers</param>
        /// <returns>string representing the headers, each on a separate line, name and value separated by :</returns>
        public static string HeaderList(NameValueCollection headers) => HeaderListWithout(headers, new List<string>());

        /// <param name="headers">a name value collection representing HTTP headers</param>
        /// <param name="filterHeaders">headers that should in the result</param>
        /// <returns>string representing the filterHeaders, each on a separate line, name and value separated by :</returns>
        public static string HeaderList(NameValueCollection headers, IEnumerable<string> filterHeaders) =>
            filterHeaders.Aggregate(string.Empty, (current, filter) => current + MultiLineHeader(filter, headers[filter]));

        /// <param name="headers">a name value collection representing HTTP headers</param>
        /// <param name="headersToOmit">headers that should be filtered out in the result</param>
        /// <returns>string representing the headers without the headersToOmit, each on a separate line, name and value separated by :</returns>
        public static string HeaderListWithout(NameValueCollection headers, List<string> headersToOmit)
        {
            var returnValue = string.Empty;
            for (var i = 0; i < headers.Count; i++)
            {
                if (!headersToOmit.Contains(headers.Keys[i], StringComparer.CurrentCultureIgnoreCase))
                {
                    returnValue += MultiLineHeader(headers.Keys[i], headers[i]);
                }
            }
            return returnValue;
        }

   
        /// <param name="input">input to be parsed</param>
        /// <param name="defaultDomain">default domain for the cookie (used if not specified)</param>
        /// <param name="utcNow">current date and time in UTC</param>
        /// <param name="defaultPath">default path for the cookie (used if not specified)</param>
        /// <returns>a CookieCollection representing the cookie specification in the input</returns>
        public static CookieCollection ParseCookies(string input, string defaultDomain, DateTime utcNow, string defaultPath = "/")
        {
            var collection = new CookieCollection();
            var lines = input.SplitLines();
            foreach (var line in lines)
            {
#if NET48
                var cookieText = UpdateExpiresFromMaxAge(line, utcNow);

                if (HttpCookie.TryParse(cookieText, out var httpCookie))
                {
                    var cookie = new Cookie
                    {
                        Name = httpCookie.Name,
                        Value = httpCookie.Value,
                        Domain = httpCookie.Domain ?? defaultDomain,
                        Expires = httpCookie.Expires,
                        Path = httpCookie.Path,
                        HttpOnly = httpCookie.HttpOnly,
                        Secure = httpCookie.Secure
                    };
                    if (string.IsNullOrEmpty(cookie.Domain))
                    {
                        throw new ArgumentException($"Set CookieDomain or specify domain in the cookie specification for '{line}'");
                    }
                    collection.Add(cookie);
                }
                else
                {
                    throw new ArgumentException($"Could not parse '{line}' as a cookie");
                }
#else
                var cookieText = RemoveInvalidCookieOptions(line);
                cookieText = UpdateExpiresFromMaxAge(cookieText, utcNow);
            
                if(SetCookieHeaderValue.TryParse(cookieText, out var httpCookie))
                {
                    var cookie = new Cookie
                    {
                        Name = httpCookie.Name.Value,
                        Value = httpCookie.Value.Value,
                        Domain = httpCookie.Domain.Value ?? defaultDomain,
                        Path = httpCookie.Path.Value ?? defaultPath,
                        HttpOnly = httpCookie.HttpOnly,
                        Secure = httpCookie.Secure
                    };
                    
                    if (httpCookie.Expires != null)
                    {
                        cookie.Expires = httpCookie.Expires.Value.UtcDateTime;
                    }

                    if (string.IsNullOrEmpty(cookie.Domain))
                    {
                        throw new ArgumentException($"Set CookieDomain or specify domain in the cookie specification for '{line}'");
                    }
                    collection.Add(cookie);
                }
                else
                {
                    throw new ArgumentException($"Could not parse '{line}' as a cookie");
                }
#endif
            }
            return collection;
        }

        /// <param name="input">string to be parsed, with key value pairs on separate lines</param>
        /// <returns>parsed NameValueCollection</returns>
        public static NameValueCollection ParseNameValueCollection(string input)
        {
            var collection = new NameValueCollection();
            var lines = input.SplitLines();
            foreach (
                var keyValuePair in
                lines.Select(line => line.ParseKeyValuePair()).Where(keyValuePair => keyValuePair.Key != null))
            {
                collection.Set(keyValuePair.Key, keyValuePair.Value);
            }
            return collection;
        }

        /// <param name="input">input string</param>
        /// <param name="replacement">value to replace newlines with</param>
        /// <returns>the input string with newlines replaced by the replacement value</returns>
        public static string ReplaceNewLines(string input, string replacement) => Regex.Replace(input, @"\r\n?|\n", replacement);

#endregion

#region Private Methods

        /// <param name="header">header name</param>
        /// <param name="value">value of the header</param>
        /// <returns>a string containing header name and value, separated by :, and ending with new line</returns>
        private static string MultiLineHeader(string header, string value) => $"{header}: {value}\n";

        /// <param name="cookieText">the original cookie text</param>
        /// <param name="utcNow">current date and time in UTC</param>
        /// <remarks>the HttpCookie parser does not recognize the Max-Age attribute, so if it's there, we morph it into an Expires attribute
        /// If Expires was there already, it is overwritten as per the spec (https://tools.ietf.org/html/rfc6265#section-4.1)</remarks>
        /// <returns>new cookie text, with an Expires property instead of Max-Age if the Max-Age existed in the original</returns>
        private static string UpdateExpiresFromMaxAge(string cookieText, DateTime utcNow)
        {

            // Try finding the max-age attribute with its integer value in Group[1].
            var matchMaxAge = new Regex("\\bmax-age=(\\d*)\\b", RegexOptions.IgnoreCase).Match(cookieText);
            if (!matchMaxAge.Success) return cookieText;
            var maxAge = Convert.ToInt64(matchMaxAge.Groups[1].Value);
            // Transform maxAge into an expires attribute and use the format as per the specification
            var expires = (utcNow + TimeSpan.FromSeconds(maxAge)).ToString("R");
            // If the expires attribute was there already, replace it
            var regex = new Regex("\\bexpires=(.*?)((;)|($))", RegexOptions.IgnoreCase);
            if (regex.IsMatch(cookieText))
            {
                return regex.Replace(cookieText, match =>
                {
                    var dateSpec = match.Groups[1];
                    return $"{match.Value.Substring(0, dateSpec.Index - match.Index)}" +
                           $"{expires}{match.Value.Substring(dateSpec.Index - match.Index + dateSpec.Length)}";
                });
            }
            // If not, add it.
            return cookieText + "; Expires=" + expires;
        }

#if NET5_0

        /// <summary>
        /// Validate the input cookie string and remove the invalid field/option.
        /// </summary>
        /// <param name="cookieText">The original cookie text.</param>
        /// <returns>The updated cookie text.</returns>
        private static string RemoveInvalidCookieOptions(string cookieText)
        {
            var fields = cookieText.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var stringBuilder = new StringBuilder(fields[0]);
            if(fields.Length > 1)
            {
                for(int index = 1; index < fields.Length; index++)
                {
                    var trimedField = fields[index].Trim();
                    if(string.IsNullOrEmpty(trimedField))
                        continue;

                    var kvp = trimedField.Split('=');
                    if(kvp.Length > 0 && _cookieOptions.Contains(kvp[0].ToLower()))
                    {
                        stringBuilder.Append($";{trimedField}");
                    }
                }
            }

            return stringBuilder.ToString();
        }
#endif
#endregion
    }

}
