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

using System.Diagnostics.CodeAnalysis;

namespace RestTests
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Needed for testing"),
     SuppressMessage("ReSharper", "NotAccessedField.Global", Justification = "needed for testing"),
     SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class FewTypes
    {
        public bool BoolValue;
        public byte ByteValue;
        public char CharValue;
        public double DoubleValue;
        public int IntValue;
        public long LongValue;
        public string StringValue;

        public FewTypes()
        {
        }

        public FewTypes(bool init)
        {
            if (!init) return;
            ByteValue = 255;
            IntValue = 2147483647;
            LongValue = -223372036854775808;
            BoolValue = true;
            DoubleValue = 3.1415926535;
            StringValue = "string value";
            CharValue = 'c';
        }
    }
}