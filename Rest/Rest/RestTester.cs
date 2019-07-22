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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Rest.ContentObjects;
using Rest.Model;
using Rest.Utilities;

namespace Rest
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global", Justification = "Enumerables not understood by FitSharp")]
    public class RestTester
    {
        private readonly ContentObjectFactory _contentObjectFactory;
        private readonly RestSession _session;

        [Documentation("Script fixture for REST testing, instantiated with endpoint URL")]
        public RestTester(string endPoint)
        {
            // Taking a dependency on the injector since this is an entry point for FitNesse, 
            // so we don't want the dependencies to be injected via the constructor here
            _session = Injector.InjectRestSession(endPoint);
            _contentObjectFactory = Injector.InjectContentObjectFactory();
        }

        [Documentation("Script fixture for REST testing, not instantiated with endpoint URL")]
        public RestTester() : this(null)
        {
        }

        [Documentation("Set / get endpoint for the service(base URL)")]
        public string EndPoint
        {
            set => _session.EndPoint = new Uri(value);
            get => _session.EndPoint?.OriginalString;
        }

        [Documentation("Set/get the payload for the REST request")]
        public string RequestBody
        {
            get => _session.Body;
            set => _session.Body = value;
        }

        [Documentation("Get all cookie names and values in the request (for debugging)")]
        public string RequestCookies => FitNesseFormatter.CookieList(_session.Request?.Cookies);

        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Interface to FitNesse")]
        [Documentation("The absolute URI used for the request")]
        public string RequestUri => _session?.Request?.RequestUri.AbsoluteUri;

        [Documentation("The HTTP response code of the REST request")]
        public int ResponseCode => (int) _session.Response.StatusCode;

        [Documentation("Description of the HTTP response code")]
        public string ResponseCodeDescription => _session.Response.StatusDescription;

        [Documentation("Try to create a JSON, XML or Text object from the response by parsing the response string")]
        public ContentObject ResponseObject
        {
            get
            {
                var contentType = _session.Response.GetResponseHeader("Content-Type");
                return _contentObjectFactory.Create(contentType, _session.ResponseText);
            }
        }

        [Documentation("Get a property of a cookie (on name or index) in the response. All public properties of the C# Cookie class can be used")]
        public object PropertyOfResponseCookie(string propertyName, object cookieName)
        {
            Cookie cookie;
            if (cookieName is int id)
            {
                cookie = _session.Response.Cookies[id];
            }
            else
            {
                cookie = _session.Response.Cookies[cookieName.ToString()];
            }
            var method = typeof(Cookie).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return method?.GetValue(cookie);
        }

        [Documentation("All request headers separated by newlines")]
        public string RequestHeaders() => FitNesseFormatter.HeaderList(_session.Request.Headers);

        [Documentation("All request headers in the list separated by newlines")]
        public string RequestHeaders(List<string> requiredHeaders) => FitNesseFormatter.HeaderList(_session.Request.Headers, requiredHeaders);

        [Documentation("The request headers except those specified in the list")]
        public string RequestHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Request.Headers, headersToOmit);

        [Documentation("The response payload (serialized to string). Can only be used after executing a Send To command")]
        public string Response() => _session.ResponseText;

        [Documentation("Get all cookies in the response")]
        public string ResponseCookies() => FitNesseFormatter.CookieList(_session.Response.Cookies);

        [Documentation("All response headers")]
        public string ResponseHeaders() => FitNesseFormatter.HeaderList(_session.Response.Headers);

        [Documentation("All response headers in the list separated by newlines")]
        public string ResponseHeaders(List<string> requiredHeaders) =>
            FitNesseFormatter.HeaderList(_session.Response.Headers, requiredHeaders);

        [Documentation("The response headers except for those in the list")]
        public string ResponseHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Response.Headers, headersToOmit);

        [Documentation("Send a REST request to an endpoint")]
        public bool SendTo(string requestType, string resource) => _session.MakeRequest(requestType, resource);

        [Documentation("Send a REST request to an endpoint using specified payload")]
        public bool SendToWithBody(string requestType, string resource, string body)
        {
            _session.Body = body;
            return _session.MakeRequest(requestType, resource);
        }

        [Documentation("Set a request header")]
        public void SetRequestHeaderTo(string header, string value) => _session.RequestHeadersToAdd[header] = value;

        [Documentation(
            "Extracts a value from a request header using a regular expression (regex) matcher. In the expression, use parentheses () to indicate the section to be extracted")]
        public string ValueFromRequestHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.RequestHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        [Documentation(
            "Extracts a value from a response header using a regular expression (regex) matcher. In the expression, use parentheses () to indicate the section to be extracted")]
        public string ValueFromResponseHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.ResponseHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        [Documentation("Extracts a value from a response using a matcher. It uses Regex, XPath or JSON query based on the Content-Type")]
        public string ValueFromResponseMatching(string matcher)
        {
            var responseContentType = _session.Response.GetResponseHeader("Content-Type");
            return _contentObjectFactory.Create(responseContentType, _session.ResponseText).Evaluate(matcher);
        }

        [Documentation("Returns the version info of the fixture. " +
                       "SHORT: just the version, EXTENDED: name, version, description, copyright. Anything else: name, version")]
        public static string VersionInfo(string qualifier) => ApplicationInfo.VersionInfo(qualifier);
    }
}