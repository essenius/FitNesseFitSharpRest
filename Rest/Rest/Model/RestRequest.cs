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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Rest.Model
{
    internal class RestRequest
    {
        private readonly SessionContext _context;
        private readonly HttpWebRequest _request;

        public RestRequest(HttpWebRequest request, SessionContext context)
        {
            Debug.Assert(request != null, "request != null");
            Debug.Assert(context != null, "context != null");
            _request = request;
            _context = context;
            _context.SetDefaults(_request);
        }

        public CookieCollection Cookies => _request.CookieContainer.GetCookies(RequestUri);

        public NameValueCollection Headers => _request.Headers;

        public Uri RequestUri => _request.RequestUri;

        public static bool SupportsBody(string method) => method != WebRequestMethods.Http.Get && method != WebRequestMethods.Http.Head;

        public virtual HttpWebResponse Execute(string method, string body)
        {
            _request.Method = method;
            HttpWebResponse response;
            try
            {
                SetBody(body, _context.RequestEncoding, _request.Method);
                response = (HttpWebResponse) _request.GetResponse();
            }
            catch (WebException we)
            {
                // if we got a response error, give that back
                // if we couldn't even send the request, rethrow the exception
                response = (HttpWebResponse) we.Response;
                if (response == null)
                {
                    throw;
                }
            }
            return response;
        }

        public string HeaderValue(string header) => _request.Headers[header] ?? string.Empty;

        private void SetBody(string body, Encoding encoding, string method)
        {
            {
                // If the stream is empty, or if this is a GET/HEAD, don't send a request stream
                // (those two don't support request streams)
                if (!string.IsNullOrEmpty(body) && SupportsBody(method))
                {
                    var bytes = encoding.GetBytes(body);
                    _request.ContentLength = bytes.Length;
                    // WebException trapped in calling method
                    using (var writeStream = _request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    _request.ContentLength = 0;
                }
            }
        }

        public virtual void UpdateHeaders(NameValueCollection requestHeadersToAdd)
        {
            foreach (var entry in requestHeadersToAdd.AllKeys)
            {
                // if it doesn't exist already, it's probably non-standard.
                // we try adding it, and if it was standard an ArgumentException is thrown
                if (_request.Headers[entry] == null)
                {
                    _request.Headers.Add(entry, requestHeadersToAdd[entry]);
                }
                else
                {
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
                            throw new ArgumentException("Unrecognized standard header: " + requestHeadersToAdd[entry] + "This was not expected to happen.");
                    }
                }
            }
        }
    }
}