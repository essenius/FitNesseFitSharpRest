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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Rest.Utilities;

namespace Rest.Model
{
    internal class SessionContext
    {
        public SessionContext()
        {
            /* CookieContainer = new CookieContainer(); */
            Credentials = CredentialCache.DefaultCredentials;
            DefaultAccept = "application/xml,application/json;q=0.9,*/*;q=0.8";
            DefaultContentType = "application/json";
            Proxy = WebRequest.GetSystemWebProxy();
            RequestEncoding = Encoding.GetEncoding("iso-8859-1");
            DefaultUserAgent = "FitNesseRest";
            DefaultXmlNameSpaceKey = "atom";
            Timeout = 10D; // seconds
            TrimWhitespace = false;
            XmlValueTypeAttribute = string.Empty;
            ContentTypeMap = new NameValueCollection();
            ContentTypeMap.Set("application/xml", "xml");
            ContentTypeMap.Set("application/json", "json");
            ContentTypeMap.Set("text/plain", "text");
            ContentTypeMap.Set("multipart/mixed", "text");
            ContentTypeMap.Set("default", "json");
            Headers = new NameValueCollection();
            // SystemDefault doesn't enable TLS 1.2, and we want at least that by default.
            // See e.g. https://stackoverflow.com/questions/28286086/default-securityprotocol-in-net-4-5
            // By using |=, we ensure this is future proof - it will use the highest.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private NameValueCollection ContentTypeMap { get; }
        private string CookiesList { get; set; }
        /* private CookieContainer CookieContainer { get; } */
        private ICredentials Credentials { get; }
        private string DefaultAccept { get; set; }
        private string DefaultContentType { get; set; }
        private string DefaultUserAgent { get; set; }
        public string DefaultXmlNameSpaceKey { get; private set; }
        private NameValueCollection Headers { get; }
        private IWebProxy Proxy { get; set; }
        public Encoding RequestEncoding { get; private set; }
        private double Timeout { get; set; }
        public bool TrimWhitespace { get; set; }
        public string XmlValueTypeAttribute { get; private set; }

        [SuppressMessage("Performance", "CA1822:Mark members as static",
            Justification = "Consistency with other properties, and hiding of side effect")]
        public string SecurityProtocol
        {
            get => $"{ServicePointManager.SecurityProtocol}";
            set
            {
                const bool ignoreCase = true;
                var protocol = (SecurityProtocolType) Enum.Parse(typeof(SecurityProtocolType), value, ignoreCase);
                ServicePointManager.SecurityProtocol = protocol;
            }
        }

        /// <summary>Determine the type of content handler we need  based on the content type</summary>
        /// <param name="contentType">the content type of the payload</param>
        /// <returns>the content handler (xml, json, text)</returns>
        public string ContentHandler(string contentType)
        {
            var contentHandler = ContentTypeMap.Get(contentType);
            return string.IsNullOrEmpty(contentHandler) ? ContentTypeMap.Get("default") : contentHandler;
        }

        /// <summary>Set session context parameter based on an identifier</summary>
        /// <param name="key">
        ///     the identifier indicating the parameter (case insensitive).
        ///     Allowed are DefaultAccept, DefaultContentType, Encoding, Proxy, Timeout, TrimWhitespace, DefaultUserAgent,
        ///     DefaultUserAgent,
        ///     DefaultXmlNamespaceKey, XmlValueTypeAttribute, Headers, ContentTypeMapping, Cookies, SecurityProtocol
        /// </param>
        /// <param name="value">the value to set the context parameter to</param>
        /// <returns>true if successful, false if not</returns>
        public bool SetConfig(string key, string value)
        {
            var actionDictionary = new Dictionary<string, Func<string, bool>>
            {
                {
                    @"DEFAULTACCEPT", v =>
                    {
                        DefaultAccept = v;
                        return true;
                    }
                },
                {
                    @"DEFAULTCONTENTTYPE", v =>
                    {
                        DefaultContentType = v;
                        return true;
                    }
                },
                {
                    @"ENCODING", v =>
                    {
                        RequestEncoding = Encoding.GetEncoding(v);
                        return true;
                    }
                },
                {@"PROXY", SetProxy},
                {
                    @"TIMEOUT", v =>
                    {
                        Timeout = double.Parse(v);
                        return true;
                    }
                },
                {
                    @"DEFAULTUSERAGENT", v =>
                    {
                        DefaultUserAgent = v;
                        return true;
                    }
                },
                {
                    @"DEFAULTXMLNAMESPACEKEY", v =>
                    {
                        DefaultXmlNameSpaceKey = v;
                        return true;
                    }
                },
                {
                    @"XMLVALUETYPEATTRIBUTE", v =>
                    {
                        XmlValueTypeAttribute = v;
                        return true;
                    }
                },
                {
                    @"HEADERS", v =>
                    {
                        Headers.Add(FitNesseFormatter.ParseNameValueCollection(value));
                        return true;
                    }
                },
                {
                    @"CONTENTTYPEMAPPING", v =>
                    {
                        ContentTypeMap.Add(FitNesseFormatter.ParseNameValueCollection(v));
                        return true;
                    }
                },
                {
                    @"COOKIES", v =>
                    {
                        CookiesList = v;
                        /* var cookies = FitNesseFormatter.ParseCookies(v, CookieDomain, DateTime.UtcNow);
                        CookieContainer.Add(cookies); */
                        return true;
                    }
                },
                {
                    @"SECURITYPROTOCOL", v =>
                    {
                        SecurityProtocol = v;
                        return true;
                    }
                },
                {
                    @"TRIMWHITESPACE", v =>
                    {
                        TrimWhitespace = bool.Parse(v);
                        return true;
                    }
                }
            };

            var upperKey = key.ToUpperInvariant();
            return actionDictionary.ContainsKey(upperKey) && actionDictionary[upperKey](value);
        }

        /// <summary>Set the defaults for a request based on the context settings</summary>
        /// <param name="request">the request to be updated (must be valid)</param>
        public void SetDefaults(HttpWebRequest request)
        {
            request.Credentials = Credentials;
            request.CookieContainer = new CookieContainer();
            if (!string.IsNullOrEmpty(CookiesList))
            {
                var defaultDomain = request.RequestUri.Host;
                var cookies = FitNesseFormatter.ParseCookies(CookiesList, defaultDomain, DateTime.UtcNow);
                request.CookieContainer.Add(cookies);
            }
            request.Accept = DefaultAccept;
            request.ContentType = DefaultContentType;
            request.Proxy = Proxy;
            request.UserAgent = DefaultUserAgent;
            request.Headers.Add(Headers);
            request.Timeout = (int) (Timeout * 1000 + 0.5);
        }

        /// <summary>Set the proxy</summary>
        /// <param name="value">"System": use system proxy; "None": use no proxy; URL: use the URL as proxy</param>
        /// <returns>true if successful, false if not</returns>
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
