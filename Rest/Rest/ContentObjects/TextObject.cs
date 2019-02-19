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

        public TextObject(object content)
        {
            if (!(content is string))
            {
                throw new NotImplementedException("binary objects not supported for Text object creation");
            }
            _content = content.ToString();
        }

        public static bool IsValid(string input) =>
            !input.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');

        internal override bool AddAt(ContentObject objToAdd, string locator) => throw new NotImplementedException();

        internal override bool Delete(string locator) => SetProperty(locator, string.Empty);

        internal override string Evaluate(string matcher)
        {
            // Singleline is a bit of a misnomer, it means that . also matches cr and lf
            // and we need that to make the MatchGroupPattern work across multiple lines
            var regex = new Regex(matcher, RegexOptions.Singleline);
            var match = regex.Match(_content);
            return match.Success ? match.Groups[1].Value : null;
        }

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

        internal override string GetProperty(string locator) => Evaluate(locator);

        internal override string GetPropertyType(string locator) => GetProperty(locator)?.CastToInferredType()?.GetType().ToString();

        internal override string Serialize() => _content;

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