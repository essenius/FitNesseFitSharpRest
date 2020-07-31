﻿// Copyright 2015-2020 Rik Essenius
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

        /// <summary>Reports on properties in query or table table. This constructor is not for decision tables - locator will be ignored if you do</summary>
        /// <param name="contentObject">the object to get the properties for</param>
        /// <param name="locator">the specification of the property filter</param>
        [Documentation("Reports on properties in query or table table. This constructor is not for decision tables - locator will be ignored if you do")]
        public PropertiesForObject(ContentObject contentObject, string locator)
        {
            _contentObject = contentObject;
            _locator = locator;
        }

        /// <summary>Reports on properties in decision, query or table table.</summary>
        /// <param name="contentObject">the object to get the properties for</param>
        [Documentation("Reports on properties in decision, query or table table.")]
        public PropertiesForObject(ContentObject contentObject) : this(contentObject, string.Empty)
        {
        }

        /// <summary>Decision column: XPath, JPath or regular expression to identify the property (based on the type of object)</summary>
        [SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "Test case")]
        [Documentation("Decision column: XPath, JPath or regular expression to identify the property (based on the type of object)")]
        public string Property { set; private get; }

        /// <summary>Decision column: The property type. Exact values depend on object type (JSON, XML, Text). 
        /// Text objects infer the type from the value </summary>
        [Documentation("Decision column: The property type. Exact values depend on object type (JSON, XML, Text). Text objects infer the type from the value")]
        public string Type { private set; get; }

        /// <summary>Decision column: Value of the property (output only for Text objects)</summary>
        [Documentation("Decision column: Value of the property (output only for Text objects)")]
        public string Value { set; get; }

        /// <summary>Decision column: Whether the value of the property was changed by this line</summary>
        [Documentation("Decision column: Whether the value of the property was changed by this line")]
        public bool ValueWasSet { get; private set; }

        private static string Report(string input) => "report:" + input;

        /// <summary>Table interface returning properties for the object</summary>
        /// <param name="table">Ignored, only used to satisfy the Table Table interface</param>
        /// <requires>content object and locator were specified</requires>
        /// <returns>a list in the format that the Table Table interface expects, containing all the properties, types and values</returns>
        [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Table Table interface")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Table Table interface")]
        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Table Table interface")]
        [Documentation("Table interface returning properties for the object")]
        public List<object> DoTable(List<List<string>> table)
        {
            var returnList = _contentObject.GetProperties(_locator).Select(property => new List<string>
            {
                Report(property),
                Report(_contentObject.GetPropertyType(property)),
                Report(_contentObject.GetProperty(property))
            }).Cast<object>().ToList();
            returnList.Insert(0, new List<string> {Report("Property"), Report("Type"), Report("Value")});
            return returnList;
        }

        /// <summary>Support for the Decision interface returning one property value</summary>
        /// <requires>content object and locator were specified</requires>
        /// <guarantees>
        /// If Property is not null
        ///     if Value is not null, sets the Property value and raise flag that value was set
        ///     gets the Property value and type
        /// </guarantees>
        [Documentation("Support for the Decision interface returning one property value")]
        public void Execute()
        {
            ValueWasSet = false;
            if (Property == null) return;
            if (Value != null)
            {
                ValueWasSet = _contentObject.SetProperty(Property, Value);
            }
            Value = _contentObject.GetProperty(Property);
            Type = _contentObject.GetPropertyType(Property);
        }

        /// <summary>Query interface returning properties for the object</summary>
        /// <returns>a list of all the properties (name, type, value) meeting the locator criteria as a list of lists as FitNesse requires</returns>
        [Documentation("Query interface returning properties for the object")]
        public List<object> Query()
        {
            return _contentObject.GetProperties(_locator).Select(property => new List<object>
            {
                new List<object> {"Property", property},
                new List<object> {"Type", _contentObject.GetPropertyType(property)},
                new List<object> {"Value", _contentObject.GetProperty(property)}
            }).Cast<object>().ToList();
        }

        /// <summary>Support for the Decision interface; new row start</summary>
        /// <guarantees>Property, Type and Value are Null</guarantees>
        [Documentation("Support for the Decision interface; new row start")]
        public void Reset()
        {
            Property = null;
            Type = null;
            Value = null;
        }
    }
}