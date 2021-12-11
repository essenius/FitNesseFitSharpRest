// Copyright 2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

// TODO: eliminate duplicate in FixtureExplorer

using System.IO;
using System.Xml;

namespace Rest.Utilities
{
    internal class AssemblyLocator
    {
        private readonly string _assemblyName;
        private readonly string _baseFolder;

        public AssemblyLocator(string assemblyName, string baseFolder)
        {
            _assemblyName = assemblyName;
            _baseFolder = baseFolder;
        }

        public string FindAssemblyPath()
        {
            if (File.Exists(_assemblyName)) return _assemblyName;
            foreach (var xmlFile in Directory.GetFiles(_baseFolder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                var xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(xmlFile);
                }
                catch (XmlException)
                {
                    continue;
                }

                var root = xmlDoc.DocumentElement;
                if (root?.Name != "suiteConfig") continue;
                var assemblyNode = $"ApplicationUnderTest/AddAssembly[contains(., '{_assemblyName}')]";
                var xmlNode = root.SelectSingleNode(assemblyNode);
                if (xmlNode == null) continue;
                return xmlNode.InnerText;
            }

            return null;
        }
    }
}
