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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Rest.Utilities;

namespace Rest.Model
{
    internal class RestRequest
    {
        private readonly SessionContext _context;
        private readonly HttpRequestMessage _request;

        // check out https://stackoverflow.com/questions/46300390/switching-from-httpwebrequest-to-httpclient for the migration
        // see also https://stackoverflow.com/questions/4015324/how-to-implement-httprequest-with-post-method-in-c-sharp

        public RestRequest(HttpRequestMessage request, SessionContext context)
        {
            Debug.Assert(request != null, "request != null");
            Debug.Assert(context != null, "context != null");
            _request = request;
            _context = context;
            _context.SetDefaults(_request);
        }

        public CookieCollection Cookies => _context.CookieContainer.GetCookies(RequestUri);

        public Uri RequestUri => _request.RequestUri;

        /// <param name="method">the HTTP method to be checked</param>
        /// <returns>whether or not the method supports the use of a body</returns>
        public static bool SupportsBody(HttpMethod method) =>
            method != HttpMethod.Get && method != HttpMethod.Head;

        /// <summary>Executes an HTTP request</summary>
        /// <param name="method">the method to execute (must be one of the recognized HTTP methods)</param>
        /// <param name="body">the associated body (can be null)</param>
        /// <returns>the response of the request</returns>
        public virtual HttpResponseMessage Execute(HttpMethod method)
        {
            _request.Method = method;
            var response = _context.Client.SendAsync(_request).Result;
            return response;
        }

        public HttpRequestHeaders Headers => _request.Headers;

        public HttpContentHeaders ContentHeaders => _request.Content?.Headers;



        /// <summary>Sets the body if the method supports a body and if it's not empty.</summary>
        /// <param name="body">the body text to be sent (can be null)</param>
        /// <param name="encoding">the request encoding</param>
        /// <param name="method">the HTTP method we want to use</param>
        public void SetBody(string body, HttpMethod method)
        {
            {
                if (!string.IsNullOrEmpty(body) && SupportsBody(method))
                {
                    _request.Content = new StringContent(body, _context.RequestEncoding);
                }
                else
                {
                    _request.Content = null;
                }

            }
        }

        private static void SetHeader(HttpHeaders headers, string headerName, string headerValue)
        {
            if (headers == null) return;
            if (headers.Contains(headerName))
            {
                headers.Remove(headerName);
            }
            headers.Add(headerName, headerValue);
        }

        public virtual void UpdateHeaders(NameValueCollection headersToAdd)
        {
            foreach (var headerKey in headersToAdd.AllKeys)
            {
                try
                {
                    SetHeader(_request.Headers, headerKey, headersToAdd[headerKey]);
                }
                catch (InvalidOperationException)
                {
                    SetHeader(_request.Content?.Headers, headerKey, headersToAdd[headerKey]);
                }
            }
        }

        /*
        /// <summary>
        ///     Update the request headers with the values of a name value collection
        /// </summary>
        /// <param name="requestHeadersToAdd">The name value collection to take over the headers from</param>
        /// TODO delete
        public virtual void UpdateHeaders(WebHeaderCollection requestHeadersToAdd)
        {
            foreach (var entry in requestHeadersToAdd.AllKeys)
            {
                Debug.Assert(entry != null, $"{nameof(entry)} != null");
                // if it doesn't exist already, it's probably non-standard.
                // we try adding it, and if it was standard an ArgumentException is thrown
                if (_request.Headers[entry] == null)
                    _request.Headers.Add(entry, requestHeadersToAdd[entry]);
                else
                    switch (entry.ToUpperInvariant())
                    {
                        case "CONTENT-TYPE":
                            _request.ContentType = requestHeadersToAdd[entry];
                            break;
                        case "ACCEPT":
                            _request.Accept = requestHeadersToAdd[entry];
                            break;
                        case "USER-AGENT":
                            _request.UserAgent = requestHeadersToAdd[entry];
                            break;
                        default:
                            throw new ArgumentException("Unrecognized standard header: " + requestHeadersToAdd[entry] +
                                                        "This was not expected to happen.");
                    }
            }
        }
        */
    }
}
