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
using System.Net;
using System.Text;
using Rest.Utilities;

namespace Rest.Model
{
    internal class SessionContext
    {
        public SessionContext()
        {
            CookieContainer = new CookieContainer();
            Credentials = CredentialCache.DefaultCredentials;
            DefaultAccept = "application/xml,application/json;q=0.9,*/*;q=0.8";
            DefaultContentType = "application/json";
            Proxy = WebRequest.GetSystemWebProxy();
            RequestEncoding = Encoding.GetEncoding("iso-8859-1");
            DefaultUserAgent = "FitNesseRest";
            DefaultXmlNameSpaceKey = "atom";
            Timeout = 10D; // seconds
            XmlValueTypeAttribute = string.Empty;
            ContentTypeMap = new NameValueCollection();
            ContentTypeMap.Set("application/xml", "xml");
            ContentTypeMap.Set("application/json", "json");
            ContentTypeMap.Set("text/plain", "text");
            ContentTypeMap.Set("multipart/mixed", "text");
            ContentTypeMap.Set("default", "json");
            Headers = new NameValueCollection();
        }

        private NameValueCollection ContentTypeMap { get; }
        private string CookieDomain { get; set; }
        private CookieContainer CookieContainer { get; }
        private ICredentials Credentials { get; }
        private string DefaultAccept { get; set; }
        private string DefaultContentType { get; set; }
        private string DefaultUserAgent { get; set; }
        public string DefaultXmlNameSpaceKey { get; private set; }
        private NameValueCollection Headers { get; }
        private IWebProxy Proxy { get; set; }
        public Encoding RequestEncoding { get; private set; }
        private double Timeout { get; set; }
        public string XmlValueTypeAttribute { get; private set; }

        public string ContentHandler(string contentType)
        {
            var contentHandler = ContentTypeMap.Get(contentType);
            return string.IsNullOrEmpty(contentHandler) ? ContentTypeMap.Get("default") : contentHandler;
        }

        public bool SetConfig(string key, string value)
        {
            var actionDictionary = new Dictionary<string, Func<string, bool>>
            {
                {@"DEFAULTACCEPT", v => { DefaultAccept = v; return true; } },
                {@"DEFAULTCONTENTTYPE", v => { DefaultContentType = v; return true; } },
                {@"ENCODING", v => { RequestEncoding = Encoding.GetEncoding(v); return true; }},
                {@"PROXY", SetProxy},
                {@"TIMEOUT", v => { Timeout = double.Parse(v); return true; }},
                {@"DEFAULTUSERAGENT", v => { DefaultUserAgent = v; return true; }},
                {@"DEFAULTXMLNAMESPACEKEY", v => { DefaultXmlNameSpaceKey = v; return true; }},
                {@"XMLVALUETYPEATTRIBUTE", v => { XmlValueTypeAttribute = v; return true; }},
                {@"HEADERS", v => { Headers.Add(FitNesseFormatter.ParseNameValueCollection(value)); return true; }},
                {@"CONTENTTYPEMAPPING", v => { ContentTypeMap.Add(FitNesseFormatter.ParseNameValueCollection(v)); return true; }},
                {@"COOKIEDOMAIN", v => { CookieDomain = v; return true; }},
                {@"COOKIES", v => {
                    var cookies = FitNesseFormatter.ParseCookies(v, CookieDomain, DateTime.UtcNow);
                    CookieContainer.Add(cookies);
                    return true;
                }}
            };

            var upperKey = key.ToUpperInvariant();
            return actionDictionary.ContainsKey(upperKey) && actionDictionary[upperKey](value);
        }

        public void SetDefaults(HttpWebRequest request)
        {
            request.Credentials = Credentials;
            request.CookieContainer = CookieContainer;
            request.Accept = DefaultAccept;
            request.ContentType = DefaultContentType;
            request.Proxy = Proxy;
            request.UserAgent = DefaultUserAgent;
            request.Headers.Add(Headers);
            request.Timeout = (int) (Timeout * 1000 + 0.5);
        }

        private bool SetProxy(string value)
        {
            switch (value)
            {
                case "System":
                    Proxy = WebRequest.GetSystemWebProxy();
                    return true;
                case "None":
                    Proxy = new WebProxy();
                    return true;
                default:
                    if (!Uri.TryCreate(value, UriKind.Absolute, out var proxyUri)) return false;
                    Proxy = new WebProxy(proxyUri);
                    return true;
            }
        }
    }
}