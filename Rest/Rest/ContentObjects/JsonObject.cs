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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rest.Utilities;
using Formatting = Newtonsoft.Json.Formatting;

namespace Rest.ContentObjects
{
    internal class JsonObject : ContentObject
    {
        private JObject _jsonObject;


        private bool SetObject(string content)
        {
            // not using JOBject.Parse because that changes date formats
            using (var reader = new JsonTextReader(new StringReader(content)) {DateParseHandling = DateParseHandling.None})
            {
                try
                {
                    _jsonObject = JObject.Load(reader);
                    return true;
                }
                catch (JsonReaderException)
                {
                    try
                    {
                        // not an object, assume it's an array
                        var array = JArray.Load(reader);
                        _jsonObject = new JObject(new JProperty("_", array));
                        return true;
                    }
                    catch (JsonReaderException)
                    {
                        // Unable to parse as JSON
                        return false;
                    }
                }
            }
        }

        public JsonObject(object sourceObject)
        {
            if (sourceObject is string sourceString)
            {
                if (SetObject(sourceString)) return;

                // Fallback: try if this is an  XML document, and convert to JSON if so
                XmlDocument doc;
                try
                {
                    doc = sourceString.ToXmlDocument();
                }
                catch (XmlException)
                {
                    throw new ArgumentException("Unable to convert content to JSON");
                }
                SetObject(JsonConvert.SerializeXmlNode(doc));
            }
            else
            {
                _jsonObject = JObject.FromObject(sourceObject);
            }
        }

        public static bool IsValid(string input)
        {
            try
            {
                JObject.Parse(input);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        internal override bool AddAt(ContentObject objToAdd, string locator)
        {
            JObject location;
            if (string.IsNullOrEmpty(locator))
            {
                location = _jsonObject;
            }
            else
            {
                location = _jsonObject.SelectToken(locator) as JObject;
            }

            if (location == null) return false;
            location.Add(((JsonObject) objToAdd)._jsonObject.Children());
            return true;
        }

        internal override bool Delete(string locator)
        {
            if (string.IsNullOrEmpty(locator)) return false;
            var location = _jsonObject.SelectToken(locator);
            if (location == null) return false;
            // for arrays this works a bit differently than for properties
            // With arrays we need to remove the value, and with properties we need to remove the property.
            if (location.Parent is JArray arr)
            {
                arr.Remove(location);
            }
            else
            {
                location.Parent.Remove();
            }
            return true;
        }

        internal override string Evaluate(string matcher) => (string) _jsonObject.SelectToken(matcher);

        internal override IEnumerable<string> GetProperties(string locator)
        {
            var result = new List<string>();
            var tokens = _jsonObject.SelectTokens(locator);
            foreach (var token in tokens)
            {
                result.Add(token.Path);
                if (!(token is JContainer container)) continue;
                foreach (var list in container.Select(entry => GetProperties(entry.Path)))
                {
                    result.AddRange(list);
                }
            }
            return result;
        }

        internal override string GetProperty(string locator)
        {
            if (_jsonObject.SelectToken(locator) is JValue tokenValue) return tokenValue.Value?.ToString();
            var container = _jsonObject.SelectToken(locator) as JContainer;
            return container?.ToString(Formatting.None);
            //if (_jsonObject.SelectToken(locator) is JArray tokenArray)
            //{
            //    return tokenArray.ToString(Formatting.None);
                //var values = tokenArray.Select(entry => GetProperty(entry.Path)).ToList();
                //return "[" + string.Join(", ", values) + "]";
                //}
            //if (_jsonObject.SelectToken(locator) is JObject subObject) return subObject.ToString(Formatting.None);
            //return "no JValue, JArray or JObject";
        }

        internal override string GetPropertyType(string locator)
        {
            var token = _jsonObject.SelectToken(locator);
            if (token == null) return null;
            var jVal = token as JValue;
            return jVal?.Type.ToString() ?? token.Type.ToString();
        }

        internal override string Serialize() => _jsonObject?.ToString(Formatting.None);

        internal override bool SetProperty(string locator, string value)
        {
            if (!(_jsonObject.SelectToken(locator) is JValue key)) return false;

            if (key.Value == null)
            {
                // we can't deduce the type from its current value since it is null,
                // so try and deduce it from the passed value.
                key.Value = value.CastToInferredType();
                return true;
            }

            // deduce the type to be used from its current value
            var valueType = key.Value.GetType();
            key.Value = TypeDescriptor.GetConverter(valueType).ConvertFromString(value);
            return true;
        }

        public override string ToString() => "JSON Object";
    }
}