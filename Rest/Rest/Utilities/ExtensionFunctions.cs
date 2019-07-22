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
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Rest.Utilities
{
    internal static class ExtensionFunctions
    {
        public static bool IsLike(this string input, string pattern)
        {
            Debug.Assert(input != null, nameof(input) + "!= null");
            return Matches(input, "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
        }

        public static bool Matches(this string input, string pattern) =>
            new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(input);

        public static object CastToInferredType(this string input)
        {
            if (int.TryParse(input, out var intValue)) return intValue;
            if (long.TryParse(input, out var longValue)) return longValue;
            if (double.TryParse(input, out var doubleValue)) return doubleValue;
            if (bool.TryParse(input, out var boolValue)) return boolValue;
            return input;
        }

        public static KeyValuePair<string, string> ParseKeyValuePair(this string token)
        {
            const string delimiter = ":";
            string key;
            string value;
            Debug.Assert(token != null);
            if (token.Contains(delimiter))
            {
                key = token.Substring(0, token.IndexOf(delimiter, StringComparison.Ordinal)).Trim();
                value = token.Substring(token.IndexOf(delimiter, StringComparison.Ordinal) + 1).Trim();
            }
            else
            {
                key = string.Empty;
                value = token.Trim();
            }
            return new KeyValuePair<string, string>(key, value);
        }

        public static string StripAfter(this string input, string delimiter)
        {
            var position = input.IndexOf(delimiter, StringComparison.Ordinal);
            return position >= 0 ? input.Substring(0, position) : input;
        }

        public static IEnumerable<string> SplitLines(this string s)
        {
            using (var stringReader = new StringReader(s))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        // This is a secure implementation of the LoadXml function - see https://github.com/dotnet/roslyn-analyzers/issues/2477
        public static XmlDocument ToXmlDocument(this string xmlString)
        {
            using (var stringReader = new StringReader(xmlString))
            using (var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings {XmlResolver = null}))
            {
                return xmlReader.ToXmlDocument();
            }
        }

        public static XmlDocument ToXmlDocument(this XmlReader xmlReader)
        {
            var document = new XmlDocument { XmlResolver = null };
            document.Load(xmlReader);
            return document;
        }
    }
}