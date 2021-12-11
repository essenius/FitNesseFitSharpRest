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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Newtonsoft.Json;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    internal class XmlObject : ContentObject
    {
        private readonly string _defaultNameSpaceKey;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly XPathNavigator _navigator;
        private readonly string _valueTypeAttribute;
        private readonly XmlDocument _xmlDocument;

        /// <summary>Create a new XML object</summary>
        /// <param name="content">the content to be parsed to XML</param>
        /// <param name="defaultNameSpaceKey">the default namespace</param>
        /// <param name="valueTypeAttribute">the attribute used to indicate what value type we have (can be null)</param>
        /// <param name="trimWhitespace">whether or not to trim whitespace from values</param>
        public XmlObject(object content, string defaultNameSpaceKey, string valueTypeAttribute,
            bool trimWhitespace = false) : base(trimWhitespace)
        {
            var contentString = StringContent(content);
            _defaultNameSpaceKey = defaultNameSpaceKey;
            _valueTypeAttribute = valueTypeAttribute;

            // we need to create the navigator from an XML document because we want to be able to write.
            _xmlDocument = ParseContent(contentString);
            _navigator = _xmlDocument.CreateNavigator();
            Debug.Assert(_navigator != null, nameof(_navigator) + " != null");
            _navigator.MoveToFollowing(XPathNodeType.Element);
            var namespaces = _navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            Debug.Assert(namespaces != null, nameof(namespaces) + " != null");
            _namespaceManager = new XmlNamespaceManager(_navigator.NameTable);
            foreach (var entry in namespaces)
            {
                _namespaceManager.AddNamespace(string.IsNullOrEmpty(entry.Key) ? defaultNameSpaceKey : entry.Key,
                    entry.Value);
                if (string.IsNullOrEmpty(entry.Key)) DefaultNameSpace = entry.Value;
            }
        }

        private string DefaultNameSpace { get; }

        /// <summary>Convert a JSON string into an XML document. Throws an exception if it cannot be done</summary>
        /// <param name="contentString">the JSON input</param>
        /// <returns>the resulting XML document</returns>
        private static XmlDocument ConvertJsonToXml(string contentString)
        {
            XDocument node;
            try
            {
                node = JsonConvert.DeserializeXNode(contentString);
            }
            catch (JsonSerializationException)
            {
                node = JsonConvert.DeserializeXNode(contentString, "root");
            }
            catch (JsonReaderException)
            {
                // no Json.
                throw new ArgumentException("Unable to convert content to XML");
            }

            Debug.Assert(node != null, nameof(node) + " != null");
            using var xmlReader = node.CreateReader();
            return xmlReader.ToXmlDocument();
        }

        /// <summary>Convert the content to a string</summary>
        /// <param name="content">the input content</param>
        /// <returns>
        ///     the string representation of the object. If the input is a string, return that.
        ///     Else return a serialized XML representation of the object
        /// </returns>
        private static string StringContent(object content)
        {
            if (content is string) return content.ToString();
            var x = new XmlSerializer(content.GetType());
            using var sw = new StringWriter();
            x.Serialize(sw, content);
            return sw.ToString();
        }

        /// <summary>
        ///     Parse content into XML document. If the content doesn't have a root element, add that.
        ///     If the content happens to be JSON, convert it to XML. Throws an exception if parsing didn't succeed.
        /// </summary>
        /// <param name="contentString">the input string</param>
        /// <returns>the parsed XMLDocument</returns>
        private static XmlDocument ParseContent(string contentString)
        {
            try
            {
                return contentString.ToXmlDocument();
            }
            catch (XmlException xe)
            {
                if (xe.Message.Contains("multiple root elements"))
                    return ("<root>" + contentString + "</root>").ToXmlDocument();
                if (xe.Message.Contains("Data at the root level is invalid"))
                    // No XML. Try if it is JSON
                    return ConvertJsonToXml(contentString);
                throw new ArgumentException("Unable to parse content as XML");
            }
        }

        /// <param name="element">the element to find the index for</param>
        /// <returns>the element index (i.e. order number in the parent)</returns>
        private static int FindElementIndex(XPathNavigator element)
        {
            var parentNode = FindParent(element);
            if (parentNode.NodeType == XPathNodeType.Root) return 1;
            var index = 1;
            var children = parentNode.SelectChildren(XPathNodeType.Element);
            while (children.MoveNext())
            {
                Debug.Assert(children.Current != null, "children.Current != null");
                if (children.Current.Name != element.Name) continue;
                if (element.ComparePosition(children.Current) == XmlNodeOrder.Same) return index;
                index++;
            }

            throw new ArgumentException(
                "FindElementIndex: Couldn't find element within parent. This was not expected to happen.");
        }

        private static XPathNavigator FindParent(XPathNavigator node)
        {
            var xni = node.SelectAncestors(XPathNodeType.All, false);
            return xni.MoveNext() ? xni.Current : null;
        }

        /// <param name="input">an input string that might be in XML format</param>
        /// <returns>whether the input is valid XML</returns>
        public static bool IsValid(string input)
        {
            try
            {
                _ = input.ToXmlDocument();
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        /// <summary>Add an object at a certain location in the XML object</summary>
        /// <param name="objToAdd">the object to be added</param>
        /// <param name="locator">XPath query indicating the location in the XML object</param>
        /// <returns>whether the operation succeeded</returns>
        internal override bool AddAt(ContentObject objToAdd, string locator)
        {
            var node = SelectElement(locator);
            if (!node.MoveNext()) return false;

            var nav = ((XmlObject) objToAdd)._navigator.Clone();
            nav.MoveToRoot();
            nav.MoveToFollowing(XPathNodeType.Element);
            Debug.Assert(node.Current != null, "node.Current != null");
            node.Current.AppendChild(nav);
            return true;
        }

        /// <summary>Delete a certain part of the object</summary>
        /// <param name="locator">XPath query indicating the location in the XML object</param>
        /// <returns>whether the part could be removed</returns>
        internal override bool Delete(string locator)
        {
            var node = SelectElement(locator);
            if (!node.MoveNext()) return false;
            Debug.Assert(node.Current != null, "node.Current != null");
            node.Current.DeleteSelf();
            return true;
        }

        /// <summary>Evaluate the object using a matcher</summary>
        /// <param name="matcher">XPath query to be matched</param>
        /// <returns>the value that satisfies the matcher, or null if no match</returns>
        internal override string Evaluate(string matcher) => TrimIfNeeded(EvaluateInternal(matcher)?.ToString());

        private object EvaluateInternal(string matcher)
        {
            var expr = _navigator.Compile(matcher);
            expr.SetContext(_namespaceManager);
            var eval = _navigator.Evaluate(expr);
            switch (eval)
            {
                case bool _:
                    return eval;
                case string _:
                    return eval;
            }

            return eval is XPathNodeIterator iterator && iterator.MoveNext() ? iterator.Current?.InnerXml : null;
        }

        /// <summary>Construct an XPath query to find a node</summary>
        /// <param name="node">the node to create an XPath query for</param>
        /// <returns>the resulting XPath query</returns>
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases",
            Justification = "missing cases not handled (caught by default clause)")]
        private string FindXPath(XPathNavigator node)
        {
            var builder = new StringBuilder();
            while (node != null)
                switch (node.NodeType)
                {
                    case XPathNodeType.Attribute:
                        builder.Insert(0, "/@" + NodeName(node));
                        node = FindParent(node);
                        break;
                    case XPathNodeType.Element:
                        var index = FindElementIndex(node);
                        builder.Insert(0, "/" + NodeName(node) + "[" + index + "]");
                        node = FindParent(node);
                        break;
                    case XPathNodeType.Root:
                        return builder.ToString();
                    case XPathNodeType.Comment:
                        builder.Insert(0, "/comment()");
                        node = FindParent(node);
                        break;
                    default:
                        throw new ArgumentException("FindXPath: Unsupported node type");
                }

            throw new ArgumentException("FindXPath: Node was not in a document. This was not expected to happen.");
        }

        /// <summary>Get the property values satisfying the locator (can be more than one)</summary>
        /// <param name="locator">XPath query indicating the properties in the XML object</param>
        /// <returns>the properties indicated by the locator</returns>
        internal override IEnumerable<string> GetProperties(string locator)
        {
            var result = new List<string>();
            // special case: if nothing was specified, we return everything
            if (string.IsNullOrEmpty(locator)) locator = "//*";
            var element = SelectElement(locator);
            while (element.MoveNext()) result.Add(FindXPath(element.Current));
            return result;
        }

        /// <summary>Get one property value satisfying the locator</summary>
        /// <param name="locator">XPath query indicating the property in the XML object</param>
        /// <returns>the property indicated by the locator</returns>
        internal override string GetProperty(string locator)
        {
            var element = SelectElement(locator);

            return element.MoveNext() ? TrimIfNeeded(element.Current.Value) : string.Empty;
        }

        /// <summary>Get the property type satisfying the locator</summary>
        /// <param name="locator">XPath query indicating the property in the XML object</param>
        /// <returns>the property type indicated by the locator</returns>
        internal override string GetPropertyType(string locator)
        {
            var element = SelectElement(locator);
            if (!element.MoveNext()) return null;

            Debug.Assert(element.Current != null, "element.Current != null");
            if (string.IsNullOrEmpty(_valueTypeAttribute)) return element.Current.ValueType.ToString();
            var atVal = element.Current.SelectSingleNode("@" + _valueTypeAttribute, _namespaceManager);
            return atVal != null ? atVal.Value : element.Current.ValueType.ToString();
        }

        private string NodeName(XPathNavigator node)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(node.Name);
            if (node.NamespaceURI.Equals(DefaultNameSpace)) stringBuilder.Insert(0, _defaultNameSpaceKey + ":");
            return stringBuilder.ToString();
        }

        private XPathNodeIterator SelectElement(string xPath) => _navigator.Select(xPath, _namespaceManager);

        /// <returns>a serializable (string) version of the object</returns>
        internal override string Serialize() => _xmlDocument.OuterXml;

        /// <summary>Get a serialized version of the property</summary>
        /// <param name="locator">property to serialize</param>
        /// <returns>the serialized property</returns>
        internal override string SerializeProperty(string locator)
        {
            var element = SelectElement(locator);
            return element.MoveNext() ? element.Current?.InnerXml : string.Empty;
        }


        /// <summary>Set the value of a property</summary>
        /// <param name="locator">XPath query indicating the property</param>
        /// <param name="value">the new value</param>
        /// <returns>whether the operation succeeded</returns>
        internal override bool SetProperty(string locator, string value)
        {
            var element = SelectElement(locator);
            if (!element.MoveNext()) return false;
            element.Current?.SetValue(value);
            return true;
        }

        public override string ToString() => "XML Object";
    }
}
