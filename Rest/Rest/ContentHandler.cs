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

        public ContentHandler() => _contentObjectFactory = Injector.InjectContentObjectFactory();

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "Handle content in JSON, XML and TEXT format"},
            {nameof(AddToAt), "Add the content of an object into another object"},
            {
                nameof(CreateObjectFrom),
                "Create a new object from a string. It will establish the format by trying to parse it into each of the supported formats, and using the format for which parsing succeeds"
            },
            {nameof(CreateObjectFrom) + "`2", "Create a new object of the specified type (TEXT, JSON, XML) from a string"},
            {nameof(CreateObjectFromTypeInAssembly), "Create an object modeled after a class in a .Net assembly"},
            {
                nameof(CreateObjectFromTypeInAssemblyWithParams),
                "Create an object modeled after a type in a .Net assembly with constructor parameters"
            },
            {nameof(DeleteFrom), "Delete a property from an object"},
            {nameof(Evaluate), "Evaluate a query (regex for TEXT, JPath for JSON, XPath for XML)"},
            {nameof(GetClasses), "A list of the classes in an assembly"},
            {nameof(PropertiesOf), "Locators to all properties of a certain element in the object"},
            {nameof(PropertySetOfContainsValueLike), "true if one of the specified properties contains a value with the specified glob pattern"},
            {nameof(PropertyTypeOf), "Type of a property"},
            {nameof(PropertyValueOf), "Value of a property"},
            {nameof(Serialize), "A text representation of the object"},
            {nameof(SetPropertyValueOfTo), "Set the value of an existing property"}
        };

        public static bool AddToAt(ContentObject objToAdd, ContentObject baseObj, string locator) => baseObj.AddAt(objToAdd, locator);
        public static bool DeleteFrom(string locator, ContentObject obj) => obj.Delete(locator);
        public static string Evaluate(ContentObject obj, string matcher) => obj.Evaluate(matcher);

        public static List<string> GetClasses(string assembly)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(assembly));
            return asm.GetTypes().Select(type => type.Namespace + "." + type.Name).ToList();
        }

        public static bool PropertySetOfContainsValueLike(string locator, ContentObject obj, string value) =>
            obj.PropertyContainsValueLike(locator, value);

        public static IEnumerable<string> PropertiesOf(string locator, ContentObject obj) => obj.GetProperties(locator);
        public static string PropertyTypeOf(string locator, ContentObject obj) => obj.GetPropertyType(locator);
        public static string PropertyValueOf(string locator, ContentObject obj) => obj.GetProperty(locator);
        public static string Serialize(ContentObject obj) => obj.Serialize();
        public static bool SetPropertyValueOfTo(string locator, ContentObject obj, string value) => obj.SetProperty(locator, value);

        public ContentObject CreateObjectFrom(string source) => CreateObjectFrom(null, source);

        private ContentObject CreateObjectFrom(string contentType, string source) =>
            _contentObjectFactory.Create(contentType, source);

        public ContentObject CreateObjectFromTypeInAssembly(string contentType, string objectType, string assembly) =>
            CreateObjectFromTypeInAssemblyWithParams(contentType, objectType, assembly, null);

        public ContentObject CreateObjectFromTypeInAssemblyWithParams(string contentType, string objectType,
            string assembly,
            string[] parameters)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(assembly));
            var myType = asm.GetType(objectType);
            var typedParams = new List<object>();
            if (parameters != null)
            {
                typedParams.AddRange(parameters.Select(s => s.CastToInferredType()));
            }
            var instance = Activator.CreateInstance(myType, typedParams.ToArray());
            return _contentObjectFactory.Create(contentType, instance);
        }
    }
}