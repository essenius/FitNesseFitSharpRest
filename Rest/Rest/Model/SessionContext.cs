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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using Rest.Utilities;

namespace Rest.Model
{
    internal class SessionContext
    {
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;

        public SessionContext()
        {
            Credentials = CredentialCache.DefaultCredentials;
            Proxy = WebRequest.GetSystemWebProxy();
            ContentTypeMap = new NameValueCollection();
            ContentTypeMap.Set("application/xml", "xml");
            ContentTypeMap.Set("application/atom+xml", "xml");
            ContentTypeMap.Set("application/json", "json");
            ContentTypeMap.Set("text/plain", "text");
            ContentTypeMap.Set("multipart/mixed", "text");
            ContentTypeMap.Set("application/octet-stream", "unknown");
            ContentTypeMap.Set("default", "unknown");
            Headers = new NameValueCollection();

            // until .NET 4.6, SystemDefault didn't enable TLS 1.2, and we want at least that by default.
            // See e.g. https://stackoverflow.com/questions/28286086/default-securityprotocol-in-net-4-5
            // By using |=, we ensure this is future proof - it will use the highest.
            // Should no longer be needed as we moved to .NET 4.8
            //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private NameValueCollection ContentTypeMap { get; }
        private string CookiesList { get; set; }
        private ICredentials Credentials { get; }
        private string DefaultAccept { get; set; } = "application/xml,application/json;q=0.9,*/*;q=0.8";
        private string DefaultContentType { get; set; } = "application/octet-stream";
        private string DefaultUserAgent { get; set; } = "FitNesseRest";
        public string DefaultXmlNameSpaceKey { get; private set; } = "atom";
        private NameValueCollection Headers { get; }
        private IWebProxy Proxy { get; set; }
        public Encoding RequestEncoding { get; private set; } = Encoding.GetEncoding("iso-8859-1");
        private double Timeout { get; set; } = 10D; // seconds
        public bool TrimWhitespace { get; set; }
        public string XmlValueTypeAttribute { get; private set; } = string.Empty;

        public static string SecurityProtocol
        {
            get => $"{ServicePointManager.SecurityProtocol}";
            set
            {
                const bool ignoreCase = true;
                var protocol = (SecurityProtocolType)Enum.Parse(typeof(SecurityProtocolType), value, ignoreCase);
                ServicePointManager.SecurityProtocol = protocol;
            }
        }

        public CookieContainer CookieContainer => ClientHandler.CookieContainer;

        private HttpClientHandler ClientHandler =>
            _httpClientHandler ??= new HttpClientHandler
            {
                Credentials = Credentials,
                CookieContainer = new CookieContainer(),
                Proxy = Proxy,
#if NET5_0_OR_GREATER
                AutomaticDecompression = DecompressionMethods.All
#else
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
#endif
            };

        public HttpClient Client
        {
            get
            {
                if (_httpClient != null) return _httpClient;
                _httpClient = new HttpClient(ClientHandler);
                foreach (string entry in Headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(entry, Headers[entry]);
                }

                _httpClient.Timeout = TimeSpan.FromSeconds(Timeout);
                return _httpClient;
            }
        }

        /// <summary>Determine the type of content we need  based on the mime type</summary>
        /// <param name="mimeType">the content type of the payload</param>
        /// <returns>the content handler (xml, json, text)</returns>
        public string ContentType(string mimeType)
        {
            // remove all the parameters, if present
            mimeType = mimeType.StripAfter(";");

            return ContentTypeMap.Get(mimeType);
        }

        /// <summary>Get the first mime type registered for a content type</summary>
        /// <param name="contentType">json, text, or xml</param>
        /// <returns>the first mime type associated with the handler, or DefaultContentType if none</returns>
        public string MimeTypeFor(string contentType)
        {
            foreach (string mimeType in ContentTypeMap)
            {
                if (ContentTypeMap[mimeType].Equals(contentType, StringComparison.OrdinalIgnoreCase)) return mimeType;
            }

            return DefaultContentType;
        }

        public void PrepareClient()
        {
            Client.DefaultRequestHeaders.Accept.ParseAdd(DefaultAccept);
            Client.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
        }

        /// <summary>Set session context parameter based on an identifier</summary>
        /// <param name="key">
        ///     the identifier indicating the parameter (case insensitive).
        ///     Allowed are DefaultAccept, DefaultContentType, Encoding, Proxy, Timeout, TrimWhitespace, DefaultUserAgent,
        ///     DefaultUserAgent, DefaultXmlNamespaceKey, XmlValueTypeAttribute, Headers, ContentTypeMapping, Cookies,
        ///     SecurityProtocol
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
                { @"PROXY", SetProxy },
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

        /// <summary>Set the cookies for a request based on the context settings</summary>
        /// <param name="request">the request to be updated (must be valid)</param>
        public void SetCookies(HttpRequestMessage request)
        {
            if (string.IsNullOrEmpty(CookiesList)) return;
            var defaultDomain = request.RequestUri.Host;
            var cookies = FitNesseFormatter.ParseCookies(CookiesList, defaultDomain, DateTime.UtcNow);
            ClientHandler.CookieContainer.Add(cookies);
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