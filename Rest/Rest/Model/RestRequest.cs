// Copyright 2015-2023 Rik Essenius
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
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Rest.ContentObjects;

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
            _context.SetCookies(_request);
        }

        public CookieCollection Cookies => _context.CookieContainer.GetCookies(RequestUri);

        public Uri RequestUri => _request.RequestUri;

        public HttpRequestHeaders Headers => _request.Headers;

        public HttpContentHeaders ContentHeaders => _request.Content?.Headers;

        /// <summary>Executes an HTTP request</summary>
        /// <param name="method">the method to execute (must be one of the recognized HTTP methods)</param>
        /// <returns>the response of the request</returns>
        public virtual HttpResponseMessage Execute(HttpMethod method)
        {
            _request.Method = method;
            var response = _context.Client.SendAsync(_request).Result;
            return response;
        }

        /// <summary>Sets the body if the method supports a body and if it's not empty.</summary>
        /// <param name="body">the body text to be sent (can be null)</param>
        /// <param name="method">the HTTP method we want to use</param>
        public void SetBody(string body, HttpMethod method)
        {
            {
                if (!string.IsNullOrEmpty(body) && SupportsBody(method))
                {
                    _request.Content = new StringContent(body, _context.RequestEncoding);
                    var co = ContentObject.Parse(body);
                    var contentType = _context.MimeTypeFor(co.ContentType.ToString());
                    _request.Content.Headers.ContentType.MediaType = contentType;
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

        /// <param name="method">the HTTP method to be checked</param>
        /// <returns>whether or not the method supports the use of a body</returns>
        public static bool SupportsBody(HttpMethod method) =>
            method != HttpMethod.Get && method != HttpMethod.Head && method != HttpMethod.Delete;

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
    }
}