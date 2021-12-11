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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Rest.Utilities;

namespace Rest.ContentObjects
{
    internal class TextObject : ContentObject
    {
        // (?: makes sure the group is not returned
        // .*? makes the match lazy
        // triple quotes needed to ensure we end up with only one pair in the formatted string
        public const string MatchGroupPattern = "(?:{0}.*?){{{1}}}";

        private string _content;

        public TextObject(object content, bool trimWhitespace = false) : base(trimWhitespace)
        {
            if (!(content is string))
                throw new NotImplementedException("binary objects not supported for Text object creation");
            _content = content.ToString();
        }

        /// <remarks>Adding text to text objects is not supported</remarks>
        internal override bool AddAt(ContentObject objToAdd, string locator)
        {
            var textToAdd = objToAdd.Serialize();
            var currentValue = GetProperty(locator);
            SetProperty(locator, currentValue + textToAdd);
            return true;
        }

        /// <summary>Delete a part of the text</summary>
        /// <param name="locator">Regular expression indicating the part to be deleted</param>
        /// <returns>whether or not the operation succeeded</returns>
        internal override bool Delete(string locator) => SetProperty(locator, string.Empty);


        /// <summary>Evaluate the text object using a matcher</summary>
        /// <param name="matcher">Regular expression to be matched</param>
        /// <returns>the value that satisfy the matcher, or null if no match</returns>
        internal override string Evaluate(string matcher)
        {
            // Singleline is a bit of a misnomer, it means that it also matches cr and lf
            // and we need that to make the MatchGroupPattern work across multiple lines
            var regex = new Regex(matcher, RegexOptions.Singleline);
            var match = regex.Match(_content);
            return match.Success ? TrimIfNeeded(match.Groups[1].Value) : null;
        }

        /// <summary>Get the property values satisfying the locator (can be more than one)</summary>
        /// <param name="locator">Regular expression indicating the properties in the JSON object</param>
        /// <returns>the properties indicated by the locator</returns>
        internal override IEnumerable<string> GetProperties(string locator)
        {
            var regex = new Regex(locator, RegexOptions.Singleline);
            var match = regex.Match(_content);
            var returnValue = new Collection<string>();
            var i = 1;
            while (match.Success)
            {
                returnValue.Add(string.Format(MatchGroupPattern, locator, i++));
                match = match.NextMatch();
            }

            return returnValue;
        }

        /// <summary>Get one property value satisfying the locator</summary>
        /// <param name="locator">>Regular expression indicating the property in the JSON object</param>
        /// <returns>the property indicated by the locator</returns>
        internal override string GetProperty(string locator) => Evaluate(locator);

        /// <summary>Get the property type satisfying the locator</summary>
        /// <param name="locator">Regular expression indicating the property in the JSON object</param>
        /// <returns>the property type indicated by the locator</returns>
        internal override string GetPropertyType(string locator) =>
            GetProperty(locator)?.CastToInferredType()?.GetType().ToString();

        /// <summary>Check whether the input is valid text (i.e. no control characters except newlines and tabs)</summary>
        /// <param name="input">the input to be checked</param>
        /// <returns>whether the input is valid</returns>
        public static bool IsValid(string input) =>
            !input.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');

        /// <returns>a serializable (string) version of the object</returns>
        internal override string Serialize() => _content;

        /// <summary>Get a serialized version of the property</summary>
        /// <param name="locator">property to serialize</param>
        /// <returns>the serialized property</returns>
        internal override string SerializeProperty(string locator) => GetProperty(locator);

        /// <summary>Set the value of a property</summary>
        /// <param name="locator">Regular expression indicating the property</param>
        /// <param name="value">the new value</param>
        /// <returns>whether the operation succeeded</returns>
        internal override bool SetProperty(string locator, string value)
        {
            var regex = new Regex(locator);
            var match = regex.Match(_content);
            if (!match.Success) return false;
            var group = match.Groups[1];
            var begin = group.Index;
            var length = group.Length;
            _content = _content.Substring(0, begin) + value + _content.Substring(begin + length);
            return true;
        }

        public override string ToString() => "Text Object";
    }
}