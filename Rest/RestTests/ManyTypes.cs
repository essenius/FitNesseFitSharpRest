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
using System.Diagnostics.CodeAnalysis;

namespace RestTests
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    public class ManyTypes
    {
        public ManyTypes()
        {
        }

        public ManyTypes(bool init)
        {
            if (!init) return;
            ByteValue = 255;
            SByteValue = -128;
            ShortValue = 32767;
            UShortValue = 65525;
            IntValue = 2147483647;
            UIntValue = 4294967295;
            LongValue = -223372036854775808;
            ULongValue = 8446744073709551615;
            DecimalValue = 300.5m;
            BoolValue = true;
            DoubleValue = 3.1415926535;
            FloatValue = 2.71828f;
            StringValue = "string value";
            CharValue = 'c';
            StringArray = new[] {"hi", "there"};
            DateValue = new DateTime(2015, 1, 1);
            //UriValue = new Uri("http://localhost");
            GuidValue = new Guid();
            TimeSpanValue = new TimeSpan(2, 30, 0);
        }
#pragma warning disable CA1051 // Do not declare visible instance fields
        public bool BoolValue;
        public byte ByteValue;
        public char CharValue;
        public DateTime DateValue;
        public decimal DecimalValue;
        public double DoubleValue;
        public float FloatValue;
        public Guid GuidValue;
        public int IntValue;
        public long LongValue;
        public sbyte SByteValue;
        public short ShortValue;
        public string[] StringArray;
        public string StringValue;
        public TimeSpan TimeSpanValue;
        public uint UIntValue;
        public ulong ULongValue;
        public ushort UShortValue;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
