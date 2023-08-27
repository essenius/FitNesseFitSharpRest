// Copyright 2023 Rik Essenius
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

namespace Rest.ContentObjects
{
    internal class BinaryObject : ContentObject
    {
        private readonly string _content;

        public BinaryObject(object content) => _content = content.ToString();

        public override ContentType ContentType => ContentType.Unknown;

        internal override bool AddAt(ContentObject objToAdd, string locator) => throw new NotImplementedException();

        internal override bool Delete(string locator) => throw new NotImplementedException();

        internal override string Evaluate(string matcher) => throw new NotImplementedException();

        internal override IEnumerable<string> GetProperties(string locator) => throw new NotImplementedException();

        internal override string GetProperty(string locator) => throw new NotImplementedException();

        internal override string GetPropertyType(string locator) => throw new NotImplementedException();

        internal override string Serialize() => _content;

        internal override string SerializeProperty(string locator) => throw new NotImplementedException();

        internal override bool SetProperty(string locator, string value) => throw new NotImplementedException();

        public override string ToString() => "Binary Object";
    }
}