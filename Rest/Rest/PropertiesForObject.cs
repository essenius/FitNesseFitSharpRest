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

        // this constructor is not for decision tables - locator will be ignored if you do
        public PropertiesForObject(ContentObject contentObject, string locator)
        {
            _contentObject = contentObject;
            _locator = locator;
        }

        public PropertiesForObject(ContentObject contentObject) : this(contentObject, string.Empty)
        {
        }

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Reports on properties in decision, query or table interface"},
            {nameof(DoTable), "Table interface returning properties for the object"},
            {nameof(Execute), "Support for the Decision interface"},
            {nameof(Property), "Decision column: XPath, JPath or regular expression to identify the property (based on the type of object)"},
            {nameof(Query), "Query interface returning properties for the object"},
            {nameof(Reset), "Support for the Decision interface"},
            {
                nameof(Type),
                "Decision column: The property type. Exact values depend on object type (JSON, XML, Text). Text objects infer the type from the value"
            },
            {nameof(Value), "Decision column: Value of the property (output only for Text objects)"},
            {nameof(ValueWasSet), "Decision column: Whether the value of the property was changed by this line"}
        };

        // Decision Table columns
        public string Property { set; private get; }
        public string Type { private set; get; }
        public string Value { set; get; }
        public bool ValueWasSet { get; private set; }

        private static string Report(string input) => "report:" + input;

        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "Table Table interface")]
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

        // Decision Table row execution
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

        // Query Table interface
        public List<object> Query()
        {
            return _contentObject.GetProperties(_locator).Select(property => new List<object>
            {
                new List<object> {"Property", property},
                new List<object> {"Type", _contentObject.GetPropertyType(property)},
                new List<object> {"Value", _contentObject.GetProperty(property)}
            }).Cast<object>().ToList();
        }

        //Decision table new row start
        public void Reset()
        {
            Property = null;
            Type = null;
            Value = null;
        }
    }
}