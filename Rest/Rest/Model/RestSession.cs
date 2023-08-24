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
using System.Collections.Specialized;
using System.Net.Http;
using Rest.ContentObjects;
using Rest.Utilities;

namespace Rest.Model
{
    internal class RestSession
    {
        private readonly IRestRequestFactory _requestFactory;
        private string _body;
        private string _responseText;

        /// <summary>Start a new RestSession</summary>
        /// <param name="endPoint">the URL of the end point, i.e. base URL. Can be null if set separately</param>
        /// <param name="context">the session context</param>
        /// <param name="requestFactory">the factory creating RestRequest objects</param>
        public RestSession(string endPoint, SessionContext context, IRestRequestFactory requestFactory)
        {
            EndPoint = endPoint != null ? new Uri(endPoint) : null;
            Context = context;
            Request = null;
            Response = null;
            RequestHeadersToAdd = new NameValueCollection();
            _requestFactory = requestFactory;
        }

        /// <summary>
        ///     Request body. If 'set' and we have a JSON payload, removes the newlines.
        /// </summary>
        public string Body
        {
            get => _body;
            set
            {
                if (value == null)
                {
                    _body = null;
                    return;
                }

                // Json doesn't like newlines, so remove those. Leave them in for XML and Text
                var contentType = ContentObjectFactory.InferType(value);
                _body = contentType == ContentObjects.ContentHandler.Json
                    ? FitNesseFormatter.ReplaceNewLines(value, string.Empty)
                    : value;
            }
        }

        public SessionContext Context { get; }
        public Uri EndPoint { get; set; }
        public RestRequest Request { get; private set; }
        public NameValueCollection RequestHeadersToAdd { get; }

        public HeaderDictionary RequestHeaders
        {
            get
            {
                var headers = new HeaderDictionary();
                if (Request == null) return headers;
                headers.AddHeaders(Request.Headers);
                headers.AddHeaders(Request.ContentHeaders);
                return headers;
            }
        }

        public HttpResponseMessage Response { get; private set; }

        public HeaderDictionary ResponseHeaders
        {
            get
            {
                var headers = new HeaderDictionary();
                if (Response == null) return headers;
                headers.AddHeaders(Response.Headers);
                headers.AddHeaders(Response.Content.Headers);
                return headers; 
            }
        }

        /// <summary>The response text (null if no response)</summary>
        public string ResponseText
        {
            get
            {
                if (Response == null) return null;
                if (_responseText != null) return _responseText;
                _responseText = string.Empty;
                _responseText = Response.Content.ReadAsStringAsync().Result;
                return _responseText;
            }
        }

        /// <summary>
        ///     Execute a request and fetch the response
        /// </summary>
        /// <param name="method">the HTTP method (must be a valid one)</param>
        /// <param name="relativeUrl">the URL from the end point</param>
        /// <returns>true</returns>
        public bool MakeRequest(string method, string relativeUrl)
        {
            // If we had a previous run, we no longer need it. 
            Response = null;
            _responseText = null;

            Request = _requestFactory.Create(new Uri(EndPoint, relativeUrl), Context);
            var httpMethod = new HttpMethod(method);
            Request.SetBody(Body, httpMethod);
            Request.UpdateHeaders(RequestHeadersToAdd);
            RequestHeadersToAdd.Clear();
            Response = Request.Execute(httpMethod);
            return true;
        }



        public string RequestHeaderValue(string header) => FitNesseFormatter.GetHeader(RequestHeaders, header);
        public string ResponseHeaderValue(string header) => FitNesseFormatter.GetHeader(ResponseHeaders, header);
    }
}
