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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Rest.ContentObjects;
using Rest.Model;
using Rest.Utilities;

namespace Rest
{
    /// <summary>Handle content in JSON, XML and TEXT format</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class ContentHandler
    {
        private readonly ContentObjectFactory _contentObjectFactory;

        /// <summary>Handle content in JSON, XML and TEXT format</summary>
        /// <guarantees>Content object factory is set</guarantees>
        public ContentHandler() => _contentObjectFactory = Injector.InjectContentObjectFactory();

        /// <summary>Add the content of an object into another object</summary>
        /// <param name="objToAdd">object to be added</param>
        /// <param name="baseObj">object to be added to</param>
        /// <param name="locator">specification of the location in baseObj where objToAdd has to be added</param>
        /// <returns>true if successfully added, false otherwise</returns>
        public static bool AddToAt(ContentObject objToAdd, ContentObject baseObj, string locator) => baseObj.AddAt(objToAdd, locator);

        /// <param name="assembly">relative or absolute path to assembly to get classes of</param>
        /// <requires>assembly path must be valid</requires>
        /// <returns>List of the class names (including namespace)</returns>
        public static List<string> ClassesIn(string assembly)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(assembly));
            return asm.GetTypes().Select(type => type.Namespace + "." + type.Name).ToList();
        }

        /// <remarks>See ObjectFrom</remarks>
        public ContentObject CreateObjectFrom(string source) => ObjectFrom(null, source);

        /// <remarks>see ObjectFrom</remarks>
        public ContentObject CreateObjectFrom(string contentType, string source) => ObjectFrom(contentType, source);

        /// <remarks>See ObjectFromPropertyOf</remarks>
        public ContentObject CreateObjectFromPropertyOf(string locator, ContentObject contentObject)
        {
            return ObjectFromPropertyOf(locator, contentObject);
        }

        /// <remarks>See ObjectFromTypeInAssembly</remarks>
        public ContentObject CreateObjectFromTypeInAssembly(string contentType, string objectType, string assembly) =>
            ObjectFromTypeInAssemblyWithParams(contentType, objectType, assembly, null);

        /// <remarks>See ObjectFromTypeInAssemblyWithParams</remarks>
        public ContentObject CreateObjectFromTypeInAssemblyWithParams(string contentType, string objectType,
            string assembly, string[] parameters) =>
            ObjectFromTypeInAssemblyWithParams(contentType, objectType, assembly, parameters);

        /// <summary>Delete a property from an object</summary>
        /// <param name="locator">specification of the property that needs to be deleted</param>
        /// <param name="contentObject">the object that the property needs to be deleted from</param>
        /// <returns>whether or not the deletion succeeded</returns>
        public static bool DeleteFrom(string locator, ContentObject contentObject) => contentObject.Delete(locator);

        /// <remarks>Don't use. Will be removed in a future release</remarks>
        [Obsolete("Use EvaluateOn instead")]
        public static string Evaluate(ContentObject contentObject, string matcher) => EvaluateOn(matcher, contentObject);

        /// <summary>Evaluate a query (regex for TEXT, JPath for JSON, XPath for XML)</summary>
        /// <param name="matcher">the query to be evaluated</param>
        /// <param name="contentObject">the object to evaluate the query on</param>
        /// <returns>the result of the query</returns>
        public static string EvaluateOn(string matcher, ContentObject contentObject) => contentObject.Evaluate(matcher);

        /// <remarks>Don't use. Will be removed in a future release</remarks>
        [Obsolete("Use ClassesIn instead")]
        public static List<string> GetClasses(string assembly) => ClassesIn(assembly);

        /// <summary>Load an object from a file. It will establish the format (JSON, XML, TEXT) by parsing it</summary>
        /// <param name="sourceFile">the path of the file to be loaded</param>
        /// <requires>File must exist and be readable</requires>
        /// <returns>The loaded object</returns>
        public ContentObject LoadObjectFrom(string sourceFile)
        {
            var sourceText = File.ReadAllText(sourceFile);
            return CreateObjectFrom(null, sourceText);
        }

        /// <summary>
        ///     Create a new object from a string. It will establish the format by trying to parse it into each of the supported formats,
        ///     and using the format for which parsing succeeds
        /// </summary>
        /// <param name="source">object to be parsed</param>
        /// <returns>Content object representing the parsed source</returns>
        public ContentObject ObjectFrom(string source) => ObjectFrom(null, source);


        /// <summary>Create a new object of the specified type (TEXT, JSON, XML) from a string</summary>
        /// <param name="contentType">TEXT, JSON, XML, null or empty string</param>
        /// <param name="source">the TEXT, JSON or XML representation of the object</param>
        /// <guarantees>If contentType is empty, it tries to establish the right content type</guarantees>
        /// <returns>Content object representing the parsed source</returns>
        public ContentObject ObjectFrom(string contentType, string source) => _contentObjectFactory.Create(contentType, source);

        /// <summary>Create a new object from a property in another object</summary>
        /// <param name="locator">the specification of the property</param>
        /// <param name="contentObject">the object to get the property from</param>
        /// <returns>an object containing the specified property</returns>
        public ContentObject ObjectFromPropertyOf(string locator, ContentObject contentObject)
        {
            var property = SerializeProperty(locator, contentObject);
            return CreateObjectFrom(null, property);
        }

        /// <summary>Create an object modeled after a type in a .Net assembly</summary>
        /// <param name="contentType"></param>
        /// <param name="objectType">the name of the type to be created</param>
        /// <param name="assembly">the path to the assembly</param>
        /// <returns>Content object representing the object</returns>
        public ContentObject ObjectFromTypeInAssembly(string contentType, string objectType, string assembly) =>
            ObjectFromTypeInAssemblyWithParams(contentType, objectType, assembly, null);

        /// <summary>Create an object modeled after a type in a .Net assembly with constructor parameters</summary>
        /// <param name="contentType">TEXT, JSON, XML</param>
        /// <param name="objectType">>the name of the type to be created</param>
        /// <param name="assembly">the path to the assembly</param>
        /// <param name="parameters">the parameter list to use</param>
        /// <returns>Content object representing the object, using the parameter values</returns>
        public ContentObject ObjectFromTypeInAssemblyWithParams(string contentType, string objectType,
            string assembly, string[] parameters)
        {
            var relativeAssemblyPath = new AssemblyLocator(assembly, ".").FindAssemblyPath();
            var asm = Assembly.LoadFile(Path.GetFullPath(relativeAssemblyPath));
            var myType = asm.GetType(objectType);
            var typedParams = new List<object>();
            if (parameters != null) typedParams.AddRange(parameters.Select(s => s.CastToInferredType()));
            var instance = Activator.CreateInstance(myType, typedParams.ToArray());
            return _contentObjectFactory.Create(contentType, instance);
        }

        /// <param name="locator">the specification of the element</param>
        /// <param name="contentObject">the object the element is in</param>
        /// <returns>Locators to all properties of a certain element in the object</returns>
        public static IEnumerable<string> PropertiesOf(string locator, ContentObject contentObject) => contentObject.GetProperties(locator);

        /// <param name="locator">the specification of the property to look at</param>
        /// <param name="contentObject">the object to look in</param>
        /// <param name="value">the glob pattern</param>
        /// <returns>whether one of the specified properties contains a value with the specified glob pattern</returns>
        public static bool PropertySetOfContainsValueLike(string locator, ContentObject contentObject, string value) =>
            contentObject.PropertyContainsValueLike(locator, value);

        /// <param name="locator">the specification of the property</param>
        /// <param name="contentObject">the object to look in</param>
        /// <returns>the type of the property</returns>
        public static string PropertyTypeOf(string locator, ContentObject contentObject) => contentObject.GetPropertyType(locator);

        /// <param name="locator">the specification of the property</param>
        /// <param name="contentObject">the object to look in</param>
        /// <returns>the value of the property</returns>
        public static string PropertyValueOf(string locator, ContentObject contentObject) => contentObject.GetProperty(locator);

        /// <summary>Save an object to a file. If file name is empty, it uses a temporary file</summary>
        /// <param name="contentObject">the object to be saved</param>
        /// <param name="targetFile">the path of the file (can be relative)</param>
        /// <returns>the absolute path to the saved file</returns>
        public static string SaveObjectTo(ContentObject contentObject, string targetFile)
        {
            var saveText = Serialize(contentObject);
            if (string.IsNullOrEmpty(targetFile))
            {
                targetFile = Path.GetTempFileName();
            }
            File.WriteAllText(targetFile, saveText);
            return Path.GetFullPath(targetFile);
        }

        /// <param name="contentObject">the object to be represented in text</param>
        /// <returns>a serialized version of the object that can be saved or transmitted</returns>
        public static string Serialize(ContentObject contentObject) => contentObject.Serialize();

        /// <param name="locator">the specification of the property to be serialized</param>
        /// <param name="contentObject">the object to be represented in text</param>
        /// <returns>a serialized version of the object that can be saved or transmitted</returns>
        public static string SerializeProperty(string locator, ContentObject contentObject) => contentObject.SerializeProperty(locator);


        /// <summary>Set the value of an existing property</summary>
        /// <param name="locator">the specification of the property</param>
        /// <param name="contentObject">the object to set a property value of</param>
        /// <param name="value">the value to be set</param>
        /// <returns>whether or not setting the value succeeded</returns>
        public static bool SetPropertyValueOfTo(string locator, ContentObject contentObject, string value) =>
            contentObject.SetProperty(locator, value);
    }
}
