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
using Rest.Model;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    internal class ContentObjectFactory
    {
        private readonly SessionContext _sessionContext;

        public ContentObjectFactory(SessionContext context) => _sessionContext = context;

        /// <summary>
        ///     Create an object from a certain type using an input object
        /// </summary>
        /// <param name="contentType">the type (Json, Xml, Text)</param>
        /// <param name="source">the input object to derive the content object from</param>
        /// <returns>the content object</returns>
        public ContentObject Create(string contentType, object source)
        {
            var contentHandler = InferContentHandler(contentType, source);
            return contentHandler switch
            {
                ContentType.Json => new JsonObject(source, _sessionContext.TrimWhitespace),
                ContentType.Xml => new XmlObject(source, _sessionContext.DefaultXmlNameSpaceKey, _sessionContext.XmlValueTypeAttribute),
                ContentType.Text => new TextObject(source),
                _ => new BinaryObject(source)
            };
        }

        private ContentType InferContentHandler(string contentType, object content)
        {
            if (contentType != null)
            {
                if (Enum.TryParse(contentType, true, out ContentType validContentHandler)) return validContentHandler;
                // Check if we have a known mimetype (could become null)
                contentType = _sessionContext.ContentType(contentType);
                if (Enum.TryParse(contentType, true, out validContentHandler)) return validContentHandler;
            }

            // we don't know the content type, so we'll have to infer it
            return content is string ? InferType(content.ToString()) : ContentType.Unknown;
        }

        /// <summary>Infer the type of content by inspecting it</summary>
        /// <param name="input">the content to be inspected</param>
        /// <returns>the type of content Json, Xml or Text</returns>
        public static ContentType InferType(string input)
        {
            if (JsonObject.IsValid(input)) return ContentType.Json;
            if (XmlObject.IsValid(input)) return ContentType.Xml;
            if (TextObject.IsValid(input)) return ContentType.Text;
            return ContentType.Unknown;
            //throw new ArgumentException("Could not parse the input to XML, JSON or Text");
        }
    }
}