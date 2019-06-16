// Copyright 2015-2019 Rik Essenius
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
using System.Text.RegularExpressions;
using System.Web;

namespace Rest.Utilities
{
    internal static class FitNesseFormatter
    {
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
            return string.Join("\r\n", result);
        }

        public static string HeaderList(NameValueCollection headers) => HeaderListWithout(headers, new List<string>());

        public static string HeaderList(NameValueCollection headers, IEnumerable<string> filterHeaders) =>
            filterHeaders.Aggregate(string.Empty, (current, filter) => current + MultiLineHeader(filter, headers[filter]));

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

        private static string MultiLineHeader(string header, string value) => $"{header}: {value}\n";

        public static CookieCollection ParseCookies(string input, string defaultDomain, DateTime utcNow)
        {
            var collection = new CookieCollection();
            var lines = input.SplitLines();
            foreach (var line in lines)
            {
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
                        throw new ArgumentException(
                            $"Cookie domain can't be null. Set CookieDomain or specify domain in the cookie specification for '{line}'");
                    }
                    collection.Add(cookie);
                }
                else
                {
                    throw new ArgumentException($"Could not parse '{line}' as a cookie");
                }
            }
            return collection;
        }

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

        public static string ReplaceNewLines(string foreignInput, string replacement) => Regex.Replace(foreignInput, @"\r\n?|\n", replacement);

        private static string UpdateExpiresFromMaxAge(string cookieText, DateTime utcNow)
        {
            // the HttpCookie parser does not recognize the Max-Age attribute, so if it's there, we morph it into an Expires attribute
            // If Expires was there already, it is overwritten as per the spec (https://tools.ietf.org/html/rfc6265#section-4.1)

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
    }
}