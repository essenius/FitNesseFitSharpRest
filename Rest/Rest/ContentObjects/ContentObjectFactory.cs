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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Rest.Model;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    /// <summary>Types of content that can be handled</summary>
    public enum ContentHandler
    {
        /// <summary>JSON content</summary>
        Json,
        /// <summary>XML content</summary>
        Xml,
        /// <summary>Text content</summary>
        Text
    }

    internal class ContentObjectFactory
    {
        private readonly SessionContext _sessionContext;

        public ContentObjectFactory(SessionContext context) => _sessionContext = context;

        /// <summary>Infer the type of content by inspecting it</summary>
        /// <param name="input">the content to be inspected</param>
        /// <returns>the type of content Json, Xml or Text</returns>
        public static ContentHandler InferType(string input)
        {
            if (JsonObject.IsValid(input)) return ContentHandler.Json;
            if (XmlObject.IsValid(input)) return ContentHandler.Xml;
            if (TextObject.IsValid(input)) return ContentHandler.Text;
            throw new ArgumentException("Could not parse the input to XML, JSON or Text");
        }

        private ContentHandler InferContentHandler(string contentType, object content)
        {
            if (contentType == null)
            {
                if (content is string) return InferType(content.ToString());
                throw new ArgumentNullException(nameof(contentType), "Content type cannot be null when serializing binary objects");
            }
            if (Enum.TryParse(contentType, true, out ContentHandler validContentHandler)) return validContentHandler;

            // we only want the main content type - eliminate additional descriptors as feed etc.
            contentType = contentType.StripAfter(";");
            var rawContentHandler = _sessionContext.ContentHandler(contentType);
            // rawContentHandler should now be a valid content handler, so TryParse should always succeed
            var succeeded = Enum.TryParse(rawContentHandler, true, out validContentHandler);
            Debug.Assert(succeeded, "TryParse on ContentHandler succeeded");
            return validContentHandler;
        }

        /// <summary>
        /// Create an object from a certain type using an input object
        /// </summary>
        /// <param name="contentType">the type (Json, Xml, Text)</param>
        /// <param name="source">the input object to derive the content object from</param>
        /// <returns>the content object</returns>
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases", Justification = "default handles the rest")]
        public ContentObject Create(string contentType, object source)
        {
            var contentHandler = InferContentHandler(contentType, source);
            switch (contentHandler)
            {
                case ContentHandler.Json:
                    return new JsonObject(source, _sessionContext.TrimWhitespace);
                case ContentHandler.Xml:
                    return new XmlObject(source, _sessionContext.DefaultXmlNameSpaceKey, _sessionContext.XmlValueTypeAttribute);
                default:
                    return new TextObject(source);
            }
        }
    }
}