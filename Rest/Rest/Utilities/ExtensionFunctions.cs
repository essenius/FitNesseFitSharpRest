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
        /// <param name="input">the input to be matched</param>
        /// <param name="pattern">the glob pattern</param>
        /// <requires>input is not null and pattern is not null</requires>
        /// <returns>whether the input matches the pattern (using wildcards * matching any character, and ? matching one)</returns>
        public static bool IsLike(this string input, string pattern)
        {
            Debug.Assert(input != null, nameof(input) + "!= null");
            return Matches(input, "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$");
        }

        /// <param name="input">the input to be matched</param>
        /// <param name="pattern">the regex pattern to match against</param>
        /// <returns>whether or not the input matches the pattern</returns>
        public static bool Matches(this string input, string pattern) =>
            new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(input);

        /// <param name="input">string that may represent an int, long, double, or bool</param>
        /// <returns>an object with the value cast to the type it represents (can also remain string)</returns>
        public static object CastToInferredType(this string input)
        {
            if (int.TryParse(input, out var intValue)) return intValue;
            if (long.TryParse(input, out var longValue)) return longValue;
            if (double.TryParse(input, out var doubleValue)) return doubleValue;
            if (bool.TryParse(input, out var boolValue)) return boolValue;
            return input;
        }

        /// <param name="token">representation of key value pair, delimited by :</param>
        /// <requires>token is not null</requires>
        /// <returns>
        ///     a KeyValuePair representing the token. If no delimiter was found, key will be empty string and value the
        ///     trimmed token
        /// </returns>
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

        /// <param name="input">input value</param>
        /// <param name="delimiter">the string that demarcates the last part of the result</param>
        /// <returns>a string with the section after the delimiter removed</returns>
        public static string StripAfter(this string input, string delimiter)
        {
            var position = input.IndexOf(delimiter, StringComparison.Ordinal);
            return position >= 0 ? input.Substring(0, position) : input;
        }

        /// <param name="input">input string that can contain multiple lines</param>
        /// <returns>list of strings with every line from the input in a separate string</returns>
        public static IEnumerable<string> SplitLines(this string input)
        {
            using var stringReader = new StringReader(input);
            while (stringReader.ReadLine() is { } line) yield return line;
        }

        /// <param name="xmlString">string representing an XML document</param>
        /// <requires>xmlString must represent a valid XML document</requires>
        /// <returns>an XmlDocument representing the input string</returns>
        /// <remarks>
        ///     This is a secure implementation of the LoadXml function - see
        ///     https://github.com/dotnet/roslyn-analyzers/issues/2477
        /// </remarks>
        public static XmlDocument ToXmlDocument(this string xmlString)
        {
            using var stringReader = new StringReader(xmlString);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings {XmlResolver = null});
            return xmlReader.ToXmlDocument();
        }

        /// <param name="xmlReader">Reader for an XML document</param>
        /// <requires>xmlReader represents a valid XML reader</requires>
        /// <returns>XML document loaded from the reader</returns>
        public static XmlDocument ToXmlDocument(this XmlReader xmlReader)
        {
            var document = new XmlDocument {XmlResolver = null};
            document.Load(xmlReader);
            return document;
        }
    }
}
