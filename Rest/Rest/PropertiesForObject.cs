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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rest.ContentObjects;

namespace Rest
{
    /// <summary>Decision, table and query fixture for FitNesse to return/check properties of an object</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class PropertiesForObject
    {
        private readonly ContentObject _contentObject;
        private readonly string _locator;

        /// <summary>
        ///     Reports on properties in query or table table.
        ///     This constructor is not for decision tables - locator will be ignored if you do
        /// </summary>
        /// <param name="contentObject">the object to get the properties for</param>
        /// <param name="locator">the specification of the property filter</param>
        public PropertiesForObject(ContentObject contentObject, string locator)
        {
            _contentObject = contentObject;
            _locator = locator;
        }

        /// <summary>Reports on properties in decision, query or table tableF</summary>
        /// <param name="contentObject">the object to get the properties for</param>
        public PropertiesForObject(ContentObject contentObject) : this(contentObject, string.Empty)
        {
        }

        /// <summary>Decision column: XPath, JPath or regular expression to identify the property (based on the type of object)</summary>
        public string Property { set; private get; }

        /// <summary>
        ///     Decision column: The property type. Exact values depend on object type (JSON, XML, Text).
        ///     Text objects infer the type from the value
        /// </summary>
        public string Type { private set; get; }

        /// <summary>Decision column: Value of the property (output only for Text objects)</summary>
        public string Value { set; get; }

        /// <summary>Decision column: Whether the value of the property was changed by this line</summary>
        public bool ValueWasSet { get; private set; }

        /// <summary>Table interface returning properties for the object</summary>
        /// <param name="table">Ignored, only used to satisfy the Table Table interface</param>
        /// <requires>content object and locator were specified</requires>
        /// <returns>a list in the format that the Table Table interface expects, containing all the properties, types and values</returns>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Table Table interface")]
        public List<object> DoTable(List<List<string>> table)
        {
            var returnList = _contentObject.GetProperties(_locator).Select(property => new List<string>
            {
                Report(property),
                Report(_contentObject.GetPropertyType(property)),
                Report(_contentObject.GetProperty(property))
            }).Cast<object>().ToList();
            returnList.Insert(0, new List<string> { Report("Property"), Report("Type"), Report("Value") });
            return returnList;
        }

        /// <summary>Support for the Decision interface returning one property value</summary>
        /// <requires>content object and locator were specified</requires>
        /// <guarantees>
        ///     If Property is not null
        ///     if Value is not null, sets the Property value and raise flag that value was set
        ///     gets the Property value and type
        /// </guarantees>
        public void Execute()
        {
            ValueWasSet = false;
            if (Property == null) return;
            if (Value != null) ValueWasSet = _contentObject.SetProperty(Property, Value);
            Value = _contentObject.GetProperty(Property);
            Type = _contentObject.GetPropertyType(Property);
        }

        /// <summary>Query interface returning properties for the object</summary>
        /// <returns>
        ///     a list of all the properties (name, type, value) meeting the locator criteria as a list of lists as FitNesse
        ///     requires
        /// </returns>
        public List<object> Query()
        {
            return _contentObject.GetProperties(_locator).Select(property => new List<object>
            {
                new List<object> { "Property", property },
                new List<object> { "Type", _contentObject.GetPropertyType(property) },
                new List<object> { "Value", _contentObject.GetProperty(property) }
            }).Cast<object>().ToList();
        }

        private static string Report(string input) => "report:" + input;

        /// <summary>Support for the Decision interface; new row start</summary>
        /// <guarantees>Property, Type and Value are Null</guarantees>
        public void Reset()
        {
            Property = null;
            Type = null;
            Value = null;
        }
    }
}