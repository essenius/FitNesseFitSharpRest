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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rest.ContentObjects;

namespace Rest
{
    // Decision, table and query fixture for FitNesse
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class PropertiesForObject
    {
        private readonly ContentObject _contentObject;
        private readonly string _locator;

        // 
        [Documentation("Reports on properties in query or table table. This constructor is not for decision tables - locator will be ignored if you do")]
        public PropertiesForObject(ContentObject contentObject, string locator)
        {
            _contentObject = contentObject;
            _locator = locator;
        }

        [Documentation("Reports on properties in decision, query or table table.")]
        public PropertiesForObject(ContentObject contentObject) : this(contentObject, string.Empty)
        {
        }

        [SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "Test case")]
        [Documentation("Decision column: XPath, JPath or regular expression to identify the property (based on the type of object)")]
        public string Property { set; private get; }

        [Documentation("Decision column: The property type. Exact values depend on object type (JSON, XML, Text). Text objects infer the type from the value")]
        public string Type { private set; get; }

        [Documentation("Decision column: Value of the property (output only for Text objects)")]
        public string Value { set; get; }

        [Documentation("Decision column: Whether the value of the property was changed by this line")]
        public bool ValueWasSet { get; private set; }

        private static string Report(string input) => "report:" + input;

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

        [Documentation("Support for the Decision interface")]
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

        [Documentation("Support for the Decision interface; new row start")]
        public void Reset()
        {
            Property = null;
            Type = null;
            Value = null;
        }
    }
}