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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Rest.ContentObjects;
using Rest.Model;
using Rest.Utilities;

namespace Rest
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class ContentHandler
    {
        private readonly ContentObjectFactory _contentObjectFactory;

        [Documentation("Handle content in JSON, XML and TEXT format")]
        public ContentHandler() => _contentObjectFactory = Injector.InjectContentObjectFactory();

        [Documentation("Add the content of an object into another object")]
        public static bool AddToAt(ContentObject objToAdd, ContentObject baseObj, string locator) => baseObj.AddAt(objToAdd, locator);

        [Documentation("A list of the classes in an assembly")]
        public static List<string> ClassesIn(string assembly)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(assembly));
            return asm.GetTypes().Select(type => type.Namespace + "." + type.Name).ToList();
        }

        [Documentation("Create a new object from a string. It will establish the format by trying to parse it into each of the supported formats, " +
                       "and using the format for which parsing succeeds")]
        public ContentObject CreateObjectFrom(string source) => CreateObjectFrom(null, source);

        [Documentation("Create a new object of the specified type (TEXT, JSON, XML) from a string")]
        private ContentObject CreateObjectFrom(string contentType, string source) => _contentObjectFactory.Create(contentType, source);

        [Documentation("Create an object modeled after a class in a .Net assembly")]
        public ContentObject CreateObjectFromTypeInAssembly(string contentType, string objectType, string assembly) =>
            CreateObjectFromTypeInAssemblyWithParams(contentType, objectType, assembly, null);

        [Documentation("Create an object modeled after a type in a .Net assembly with constructor parameters")]
        public ContentObject CreateObjectFromTypeInAssemblyWithParams(string contentType, string objectType,
            string assembly,
            string[] parameters)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(assembly));
            var myType = asm.GetType(objectType);
            var typedParams = new List<object>();
            if (parameters != null) typedParams.AddRange(parameters.Select(s => s.CastToInferredType()));
            var instance = Activator.CreateInstance(myType, typedParams.ToArray());
            return _contentObjectFactory.Create(contentType, instance);
        }

        [Documentation("Delete a property from an object")]
        public static bool DeleteFrom(string locator, ContentObject contentObject) => contentObject.Delete(locator);

        [Obsolete("Use EvaluateOn instead")]
        public static string Evaluate(ContentObject contentObject, string matcher) => EvaluateOn(matcher, contentObject);

        [Documentation("Evaluate a query (regex for TEXT, JPath for JSON, XPath for XML)")]
        public static string EvaluateOn(string matcher, ContentObject contentObject) => contentObject.Evaluate(matcher);

        [Obsolete("Use ClassesIn instead")]
        public static List<string> GetClasses(string assembly) => ClassesIn(assembly);

        [Documentation("Load an object from a file. It will establish the format (JSON, XML, TEXT) by parsing it")]
        public ContentObject LoadObjectFrom(string sourceFile)
        {
            var sourceText = File.ReadAllText(sourceFile);
            return CreateObjectFrom(null, sourceText);
        }

        [Documentation("Locators to all properties of a certain element in the object")]
        public static IEnumerable<string> PropertiesOf(string locator, ContentObject contentObject) => contentObject.GetProperties(locator);

        [Documentation("true if one of the specified properties contains a value with the specified glob pattern")]
        public static bool PropertySetOfContainsValueLike(string locator, ContentObject contentObject, string value) => 
            contentObject.PropertyContainsValueLike(locator, value);

        [Documentation("Type of a property")]
        public static string PropertyTypeOf(string locator, ContentObject contentObject) => contentObject.GetPropertyType(locator);

        [Documentation("Value of a property")]
        public static string PropertyValueOf(string locator, ContentObject contentObject) => contentObject.GetProperty(locator);

        [Documentation("Save an object to a file.")]
        public static bool SaveObjectTo(ContentObject contentObject, String targetFile)
        {
            var saveText = Serialize(contentObject);
            File.WriteAllText(targetFile, saveText);
            return true;
        }

        [Documentation("A text representation of the object")]
        public static string Serialize(ContentObject contentObject) => contentObject.Serialize();

        [Documentation("Set the value of an existing property")]
        public static bool SetPropertyValueOfTo(string locator, ContentObject contentObject, string value) => contentObject.SetProperty(locator, value);
    }
}