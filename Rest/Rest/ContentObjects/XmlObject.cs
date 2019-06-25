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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace Rest.ContentObjects
{
    internal class XmlObject : ContentObject
    {
        private readonly string _defaultNameSpaceKey;
        private readonly XmlNamespaceManager _namespaceManager;
        private readonly XPathNavigator _navigator;
        private readonly string _valueTypeAttribute;
        private readonly XmlDocument _xmlDocument;

        public XmlObject(object content, string defaultNameSpaceKey, string valueTypeAttribute)
        {
            string contentString;
            if (content is string)
            {
                contentString = content.ToString();
            }
            else
            {
                var x = new XmlSerializer(content.GetType());
                using (var sw = new StringWriter())
                {
                    x.Serialize(sw, content);
                    contentString = sw.ToString();
                }
            }
            _defaultNameSpaceKey = defaultNameSpaceKey;
            _valueTypeAttribute = valueTypeAttribute;

            // we need to create the navigator from an XML document because we want to be able to write.
            _xmlDocument = new XmlDocument();
            try
            {
                _xmlDocument.LoadXml(contentString);
            }
            catch (XmlException xe)
            {
                if (xe.Message.Contains("multiple root elements"))
                {
                    _xmlDocument.LoadXml("<root>" + content + "</root>");
                } else if (xe.Message.Contains("Data at the root level is invalid"))
                {
                    // No XML. Try if it is JSON
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
                        // no JSON.
                        throw new ArgumentException("Unable to convert content to XML");
                    }
                    using (var xmlReader = node.CreateReader())
                    {
                        _xmlDocument.Load(xmlReader);
                    }
                }
            }
            _navigator = _xmlDocument.CreateNavigator();
            _navigator.MoveToFollowing(XPathNodeType.Element);
            var namespaces = _navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            if (namespaces == null) return;
            Debug.Assert(_navigator.NameTable != null, "navigator.NameTable != null");
            _namespaceManager = new XmlNamespaceManager(_navigator.NameTable);
            foreach (var entry in namespaces)
            {
                _namespaceManager.AddNamespace(string.IsNullOrEmpty(entry.Key) ? defaultNameSpaceKey : entry.Key, entry.Value);
                if (string.IsNullOrEmpty(entry.Key)) DefaultNameSpace = entry.Value;
            }
        }

        private string DefaultNameSpace { get; }

        private static int FindElementIndex(XPathNavigator element)
        {
            var parentNode = FindParent(element);
            if (parentNode.NodeType == XPathNodeType.Root)
            {
                return 1;
            }
            var index = 1;
            var children = parentNode.SelectChildren(XPathNodeType.Element);
            while (children.MoveNext())
            {
                if (children.Current.Name != element.Name) continue;
                if (element.ComparePosition(children.Current) == XmlNodeOrder.Same) return index;
                index++;
            }
            throw new ArgumentException("FindElementIndex: Couldn't find element within parent. This was not expected to happen.");
        }

        private static XPathNavigator FindParent(XPathNavigator node)
        {
            var xni = node.SelectAncestors(XPathNodeType.All, false);
            return xni.MoveNext() ? xni.Current : null;
        }

        public static bool IsValid(string input)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(input);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        internal override bool AddAt(ContentObject objToAdd, string locator)
        {
            var node = SelectElement(locator);
            if (!node.MoveNext()) return false;

            var nav = ((XmlObject) objToAdd)._navigator.Clone();
            nav.MoveToRoot();
            nav.MoveToFollowing(XPathNodeType.Element);
            node.Current.AppendChild(nav);
            return true;
        }

        internal override bool Delete(string locator)
        {
            var node = SelectElement(locator);
            if (!node.MoveNext()) return false;
            node.Current.DeleteSelf();
            return true;
        }

        internal override string Evaluate(string matcher) => EvaluateInternal(matcher)?.ToString();

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
            return eval is XPathNodeIterator iterator && iterator.MoveNext() ? iterator.Current.InnerXml : null;
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases", Justification = "missing cases not handled (caught by default clause)")]
        private string FindXPath(XPathNavigator node)
        {
            var builder = new StringBuilder();
            while (node != null)
            {
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
            }
            throw new ArgumentException("FindXPath: Node was not in a document. This was not expected to happen.");
        }

        internal override IEnumerable<string> GetProperties(string locator)
        {
            var result = new List<string>();
            // special case: if nothing was specified, we return everything
            if (string.IsNullOrEmpty(locator))
            {
                locator = "//*";
            }
            var element = SelectElement(locator);
            while (element.MoveNext())
            {
                result.Add(FindXPath(element.Current));
            }
            return result;
        }

        internal override string GetProperty(string locator)
        {
            var element = SelectElement(locator);
            return element.MoveNext() ? element.Current.Value : string.Empty;
        }

        internal override string GetPropertyType(string locator)
        {
            var element = SelectElement(locator);
            if (!element.MoveNext()) return null;

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

        internal override string Serialize() => _xmlDocument.OuterXml;

        internal override bool SetProperty(string locator, string value)
        {
            var element = SelectElement(locator);
            if (!element.MoveNext()) return false;
            element.Current.SetValue(value);
            return true;
        }

        public override string ToString() => "XML Object";
    }
}