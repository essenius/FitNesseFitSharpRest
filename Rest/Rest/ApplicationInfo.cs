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

using System.Globalization;
using System.Reflection;

namespace Rest
{
    internal static class ApplicationInfo
    {
        private static Assembly ThisAssembly => Assembly.GetExecutingAssembly();

        public static string ApplicationName { get; } = ThisAssembly.GetName().Name;

        public static string Copyright { get; } =
            ThisAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

        public static string Description { get; } =
            ThisAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public static string Version { get; } = ThisAssembly.GetName().Version.ToString();

        private static string ExtendedInfo => string.Format(CultureInfo.InvariantCulture, "{0} {1}. {2}. {3}",
            ApplicationName, Version, Description, Copyright);

        public static string VersionInfo(string qualifier)
        {
            return qualifier.ToUpperInvariant() switch
            {
                "SHORT" => Version,
                "EXTENDED" => ExtendedInfo,
                _ => string.Format(CultureInfo.InvariantCulture, "{0} {1}", ApplicationName, Version)
            };
        }
    }
}