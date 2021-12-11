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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rest.Model;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    /// <summary>Abstract content object</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public abstract class ContentObject
    {
        private readonly bool _trimWhitespace;

        /// <summary>Create new content object.</summary>
        /// <param name="trimWhitespace">whether whitespace in resulting property values or evaluations is trimmed</param>
        protected ContentObject(bool trimWhitespace = false) => _trimWhitespace = trimWhitespace;

        internal abstract bool AddAt(ContentObject objToAdd, string locator);
        internal abstract bool Delete(string locator);
        internal abstract string Evaluate(string matcher);
        internal abstract IEnumerable<string> GetProperties(string locator);
        internal abstract string GetProperty(string locator);
        internal abstract string GetPropertyType(string locator);

        /// <summary>Parse a string value into a concrete object. Tries to figure out itself it it is XML, JSON or TEXT</summary>
        /// <param name="input">the input string to be parsed</param>
        /// <remarks>
        ///     We need this one since FitNesse uses it to try and parse parameter values into objects
        ///     The factory knows all children anyway, and will call static IsValid to figure out whether the input text can be
        ///     parsed
        /// </remarks>
        /// <returns>the parsed ContentObject</returns>
        public static ContentObject Parse(string input)
        {
            var factory = Injector.InjectContentObjectFactory();
            return factory.Create(null, input);
        }

        internal bool PropertyContainsValueLike(string locator, string value)
        {
            var propertyList = GetProperties(locator);
            return propertyList.Any(property => GetProperty(property).IsLike(value));
        }

        internal abstract string Serialize();
        internal abstract string SerializeProperty(string locator);
        internal abstract bool SetProperty(string locator, string value);

        /// <summary>Trim return values if the trim whitespace parameter is true</summary>
        /// <remarks>
        ///     We may need trimming values sometimes because FitNesse does trimming too for the comparison values.
        ///     Leading or trailing whitespace in property values shouldn't happen too often, just making things more robust
        /// </remarks>
        /// <param name="input">raw input</param>
        /// <returns>trimmed input if trim whitespace parameter is true, input otherwise</returns>
        protected string TrimIfNeeded(string input) => _trimWhitespace ? input.Trim() : input;
    }
}
