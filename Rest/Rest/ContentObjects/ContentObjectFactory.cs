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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Rest.Model;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    public enum ContentHandlers
    {
        Json,
        Xml,
        Text
    }

    internal class ContentObjectFactory
    {
        private readonly SessionContext _sessionContext;

        public ContentObjectFactory(SessionContext context) => _sessionContext = context;

        public static ContentHandlers InferType(string input)
        {
            if (JsonObject.IsValid(input)) return ContentHandlers.Json;
            if (XmlObject.IsValid(input)) return ContentHandlers.Xml;
            if (TextObject.IsValid(input)) return ContentHandlers.Text;
            throw new ArgumentException("Could not parse the input to XML, JSON or Text");
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases", Justification = "default handles the rest")]
        public ContentObject Create(string contentType, object source)
        {
            ContentHandlers validContentHandler;
            if (contentType == null)
            {
                if (source is string)
                {
                    validContentHandler = InferType(source.ToString());
                }
                else
                {
                    throw new ArgumentNullException(nameof(contentType), "Content type cannot be null when serializing binary objects");
                }
            }
            else
            {
                if (!Enum.TryParse(contentType, true, out validContentHandler))
                {
                    // we only want the main content type - eliminate additional descriptors as feed etc.
                    contentType = contentType.StripAfter(";");
                    var rawContentHandler = _sessionContext.ContentHandler(contentType);
                    // rawContentHandler should now be a valid content handler, so TryParse should always succeed
                    var succeeded = Enum.TryParse(rawContentHandler, true, out validContentHandler);
                    Debug.Assert(succeeded, "TryParse on ContentHandler failed");
                }
            }

            switch (validContentHandler)
            {
                case ContentHandlers.Json:
                    return new JsonObject(source);
                case ContentHandlers.Xml:
                    return new XmlObject(source, _sessionContext.DefaultXmlNameSpaceKey, _sessionContext.XmlValueTypeAttribute);
                default:
                    return new TextObject(source);
            }
        }
    }
}