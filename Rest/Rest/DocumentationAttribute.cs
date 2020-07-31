﻿// Copyright 2019 Rik Essenius
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

namespace Rest
{
    /// <summary>
    /// Defines a Documentation attribute, so we can use [Documentation] to describe fixture functions.
    /// Those are used by FixtureExplorer to provide fixture documentation in FitNesse, but are also helpful as method documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class DocumentationAttribute : Attribute
    {
        public DocumentationAttribute(string message) => Message = message;
        public string Message { get; }
    }
}