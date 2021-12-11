// Copyright 2015-2021 Rik Essenius
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
using System.Net;
using System.Reflection;
using Rest.ContentObjects;
using Rest.Model;
using Rest.Utilities;

namespace Rest
{
    /// <summary>Script fixture for REST testing</summary>
    public class RestTester
    {
        private readonly ContentObjectFactory _contentObjectFactory;
        private readonly RestSession _session;


        /// <summary>Instantiate REST tester with endpoint URL</summary>
        /// <param name="endPoint">URL of the end point (base URL) of the REST server</param>
        /// <remarks>
        ///     Taking a dependency on the injector since this is an entry point for FitNesse,
        ///     so we don't want the dependencies to be injected via the constructor here
        /// </remarks>
        public RestTester(string endPoint)
        {
            _session = Injector.InjectRestSession(endPoint);
            _contentObjectFactory = Injector.InjectContentObjectFactory();
        }

        /// <summary>Instantiate REST tester without endpoint URL</summary>
        public RestTester() : this(null)
        {
        }

        /// <summary>Set/get endpoint for the service (base URL)</summary>
        public string EndPoint
        {
            set => _session.EndPoint = new Uri(value);
            get => _session.EndPoint?.OriginalString;
        }

        /// <summary>Set/get the payload for the REST request</summary>
        public string RequestBody
        {
            get => _session.Body;
            set => _session.Body = value;
        }

        /// <summary>Get all cookie names and values in the request (for debugging)</summary>
        public string RequestCookies => FitNesseFormatter.CookieList(_session.Request?.Cookies);

        /// <summary>The absolute URI used for the request</summary>
        public string RequestUri => _session?.Request?.RequestUri.AbsoluteUri;

        /// <summary>The HTTP response code of the REST request</summary>
        public int ResponseCode => (int) _session.Response.StatusCode;

        /// <summary>Description of the HTTP response code</summary>
        public string ResponseCodeDescription => _session.Response.StatusDescription;

        /// <summary>"Try to create a JSON, XML or Text object from the response by parsing the response string</summary>
        public ContentObject ResponseObject
        {
            get
            {
                var contentType = _session.Response.GetResponseHeader("Content-Type");
                return _contentObjectFactory.Create(contentType, _session.ResponseText);
            }
        }

        /// <summary>
        ///     Get a property of a cookie (on name or index) in the response. All public properties of the C# Cookie class
        ///     can be used
        /// </summary>
        /// <param name="propertyName">name or index of the cookie property</param>
        /// <param name="cookieName">name of the cookie</param>
        /// <returns>the value of the cookie property</returns>
        /// <requires>
        ///     propertyName is a valid public cookie property name; cookieName is either a valid cookie index or a valid
        ///     cookie name
        /// </requires>
        /// <guarantees>
        ///     if the cookieName is integer, uses the cookie at the speficied index, else uses the cookie with the
        ///     specified name
        /// </guarantees>
        public object PropertyOfResponseCookie(string propertyName, object cookieName)
        {
            Cookie cookie;
            if (cookieName is int id)
                cookie = _session.Response.Cookies[id];
            else
                cookie = _session.Response.Cookies[cookieName.ToString()];


            var method = typeof(Cookie).GetProperty(
                propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return method?.GetValue(cookie);
        }

        /// <summary>All request headers separated by newlines</summary>
        public string RequestHeaders() => FitNesseFormatter.HeaderList(_session.Request.Headers);

        /// <param name="requiredHeaders">the list of headers to retrieve</param>
        /// <returns>the headers and their values separated by newlines</returns>
        public string RequestHeaders(List<string> requiredHeaders) =>
            FitNesseFormatter.HeaderList(_session.Request.Headers, requiredHeaders);

        /// <summary>The request headers except those specified in the list</summary>
        /// <param name="headersToOmit">the headers not to include in the result</param>
        /// <returns>the list of headers and their values that are not in headersToOmit</returns>
        public string RequestHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Request.Headers, headersToOmit);

        /// <returns>The response payload (serialized to string). Can only be used after executing a Send To command</returns>
        public string Response() => _session.ResponseText;

        /// <returns>list of all cookies in the response, each in its own row</returns>
        public string ResponseCookies() => FitNesseFormatter.CookieList(_session.Response.Cookies);

        /// <returns>All response headers</returns>
        public string ResponseHeaders() => FitNesseFormatter.HeaderList(_session.Response.Headers);

        /// <param name="requiredHeaders">the header filter (i.e. only show the ones specified)</param>
        /// <returns>All response headers in the list separated by newlines</returns>
        public string ResponseHeaders(List<string> requiredHeaders) =>
            FitNesseFormatter.HeaderList(_session.Response.Headers, requiredHeaders);

        /// <param name="headersToOmit">the headers to exclude from the result</param>
        /// <returns>The response headers except for those in the list</returns>
        public string ResponseHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Response.Headers, headersToOmit);

        /// <summary>Send a REST request to an endpoint</summary>
        /// <param name="requestType">the HTTP request type</param>
        /// <param name="resource">the REST resource (i.e. relative URL)</param>
        /// <returns>true</returns>
        public bool SendTo(string requestType, string resource) => _session.MakeRequest(requestType, resource);

        /// <summary>Send a REST request to an endpoint using specified payload</summary>
        /// <param name="requestType">the HTTP request type</param>
        /// <param name="resource">the REST resource (i.e. relative URL)</param>
        /// <param name="body">the body text</param>
        /// <returns>true</returns>
        public bool SendToWithBody(string requestType, string resource, string body)
        {
            _session.Body = body;
            return _session.MakeRequest(requestType, resource);
        }

        /// <summary>Set a request header</summary>
        /// <param name="header">the header to be set</param>
        /// <param name="value">the new value of the header</param>
        public void SetRequestHeaderTo(string header, string value) => _session.RequestHeadersToAdd[header] = value;

        /// <param name="input">the input string</param>
        /// <returns>the input string with newline characters eliminated</returns>
        public static string StripNewLinesFrom(string input) =>
            input.Replace("\n", string.Empty).Replace("\r", string.Empty);

        /// <summary>
        ///     Extracts a value from a request header using a regular expression (regex) matcher.
        ///     In the expression, use parentheses () to indicate the section to be extracted
        /// </summary>
        /// <param name="header">the request header to inspect</param>
        /// <param name="matcher">the regular expression to use for matching</param>
        /// <returns>the extracted value</returns>
        public string ValueFromRequestHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.RequestHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        /// <summary>
        ///     Extracts a value from a response header using a regular expression (regex) matcher.
        ///     In the expression, use parentheses () to indicate the section to be extracted
        /// </summary>
        /// <param name="header">the response header to inspect</param>
        /// <param name="matcher">the regular expression to use for matching</param>
        /// <returns>the extracted value</returns>
        public string ValueFromResponseHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.ResponseHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        /// <summary>
        ///     Extracts a value from a response using a matcher. It uses Regex, XPath or JSON query based on the Content-Type
        /// </summary>
        /// <param name="matcher">Regex, XPath or JSON query based on the Content-Type</param>
        /// <returns>the extracted value from the response</returns>
        public string ValueFromResponseMatching(string matcher)
        {
            var responseContentType = _session.Response.GetResponseHeader("Content-Type");
            return _contentObjectFactory.Create(responseContentType, _session.ResponseText).Evaluate(matcher);
        }

        /// <param name="qualifier">
        ///     SHORT: just the version, EXTENDED: name, version, description, copyright. Anything else: name,
        ///     version
        /// </param>
        /// <returns>Version information of the fixture</returns>
        public static string VersionInfo(string qualifier) => ApplicationInfo.VersionInfo(qualifier);
    }
}
