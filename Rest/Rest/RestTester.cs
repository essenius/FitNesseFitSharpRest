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

        public RestTester(string endPoint)
        {
            // Taking a dependency on the injector since this is an entry point for FitNesse,
            // so we don't want the dependencies to be injected via the constructor here
            _session = Injector.InjectRestSession(endPoint);
            _contentObjectFactory = Injector.InjectContentObjectFactory();
        }

        public RestTester() : this(null)
        {
        }

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Script fixture for REST testing. Instantiated with or without endpoint URL"},
            {nameof(EndPoint), "Set/get endpoint for the service (base URL)"},
            {nameof(RequestBody), "Set/get the payload for the REST request"},
            {nameof(RequestHeaders), "All request headers separated by newlines"},
            {nameof(RequestHeaders) + "`1", "All request headers in the list separated by newlines"},
            {nameof(RequestHeadersWithout), "The request headers except those specified in the list"},
            {nameof(RequestUri), "The absolute URI used for the request"},
            {nameof(Response), "The response payload (serialized to string). Can only be used after executing a Send To command"},
            {nameof(ResponseCode), "The HTTP response code of the REST request"},
            {nameof(ResponseCodeDescription), "Description of the HTTP response code"},
            {nameof(ResponseHeaders), "All response headers"},
            {nameof(ResponseHeaders) + "`1", "All response headers in the list separated by newlines"},
            {nameof(ResponseHeadersWithout), "The response headers except for those in the list"},
            {nameof(ResponseObject), "Try to create a JSON, XML or Text object from the response by parsing the response string"},
            {nameof(SendTo), "Send a REST request to an endpoint"},
            {nameof(SendToWithBody), "Send a REST request to an endpoint using specified payload"},
            {nameof(SetRequestHeaderTo), "Set a request header"},
            {
                nameof(ValueFromRequestHeaderMatching),
                "Extracts a value from a request header using a regular expression (regex) matcher. In the expression, use parentheses () to indicate the section to be extracted"
            },
            {
                nameof(ValueFromResponseHeaderMatching),
                "Extracts a value from a response header using a regular expression (regex) matcher. In the expression, use parentheses () to indicate the section to be extracted"
            },
            {
                nameof(ValueFromResponseMatching),
                "Extracts a value from a response using a matcher. It uses Regex, XPath or JSON query based on the Content-Type"
            }
        };

        public string EndPoint
        {
            set => _session.EndPoint = new Uri(value);
            get => _session.EndPoint?.OriginalString;
        }

        public string RequestBody
        {
            get => _session.Body;
            set => _session.Body = value;
        }

        public string RequestUri => _session?.Request?.RequestUri.AbsoluteUri;
        public int ResponseCode => (int) _session.Response.StatusCode;
        public string ResponseCodeDescription => _session.Response.StatusDescription;

        public ContentObject ResponseObject
        {
            get
            {
                var contentType = _session.Response.GetResponseHeader("Content-Type");
                return _contentObjectFactory.Create(contentType, _session.ResponseText);
            }
        }

        public string RequestHeaders() => FitNesseFormatter.HeaderList(_session.Request.Headers);
        public string RequestHeaders(List<string> requiredHeaders) => FitNesseFormatter.HeaderList(_session.Request.Headers, requiredHeaders);

        public string RequestHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Request.Headers, headersToOmit);

        public string Response() => _session.ResponseText;
        public string ResponseHeaders() => FitNesseFormatter.HeaderList(_session.Response.Headers);

        public string ResponseHeaders(List<string> requiredHeaders) =>
            FitNesseFormatter.HeaderList(_session.Response.Headers, requiredHeaders);

        public string ResponseHeadersWithout(List<string> headersToOmit) =>
            FitNesseFormatter.HeaderListWithout(_session.Response.Headers, headersToOmit);

        public bool SendTo(string requestType, string resource) => _session.MakeRequest(requestType, resource);

        public bool SendToWithBody(string requestType, string resource, string body)
        {
            _session.Body = body;
            return _session.MakeRequest(requestType, resource);
        }

        public void SetRequestHeaderTo(string header, string value) => _session.RequestHeadersToAdd[header] = value;

        public string ValueFromRequestHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.RequestHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        public string ValueFromResponseHeaderMatching(string header, string matcher)
        {
            var headerValue = _session.ResponseHeaderValue(header);
            return _contentObjectFactory.Create("text", headerValue).Evaluate(matcher);
        }

        public string ValueFromResponseMatching(string matcher)
        {
            var responseContentType = _session.Response.GetResponseHeader("Content-Type");
            return _contentObjectFactory.Create(responseContentType, _session.ResponseText).Evaluate(matcher);
        }
    }
}