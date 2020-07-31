// Copyright 2015-2020 Rik Essenius
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
using Rest.Model;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    [Documentation("Abstract content object")]
    public abstract class ContentObject
    {
        /// <param name="input">the input string to be parsed</param>
        /// <remarks>
        /// We need this one since FitNesse uses it to try and parse parameter values into objects
        /// The factory knows all children anyway, and will call static IsValid to figure out whether the input text can be parsed
        /// </remarks>
        /// <returns>the parsed ContentObject</returns>
        [Documentation("Parse a string value into a concrete object. Tries to figure out itself it it is XML, JSON or TEXT")]
        public static ContentObject Parse(string input)
        {
            var factory = Injector.InjectContentObjectFactory();
            return factory.Create(null, input);
        }

        internal abstract bool AddAt(ContentObject objToAdd, string locator);
        internal abstract bool Delete(string locator);
        internal abstract string Evaluate(string matcher);
        internal abstract IEnumerable<string> GetProperties(string locator);
        internal abstract string GetProperty(string locator);
        internal abstract string GetPropertyType(string locator);
        internal abstract string Serialize();
        internal abstract bool SetProperty(string locator, string value);

        internal bool PropertyContainsValueLike(string locator, string value)
        {
            var propertyList = GetProperties(locator);
            return propertyList.Any(property => GetProperty(property).IsLike(value));
        }
    }
}