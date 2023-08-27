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

using System.Collections.Specialized;
using System.Net.Http;
using Rest.Model;

namespace RestTests
{
    internal class RestRequestMock : RestRequest
    {
        public RestRequestMock(HttpRequestMessage request, SessionContext context) : base(request, context) =>
            ExecuteWasCalled = false;

        public bool ExecuteWasCalled { get; private set; }

        public override HttpResponseMessage Execute(HttpMethod method)
        {
            ExecuteWasCalled = true;
            return null;
        }

        public override void UpdateHeaders(NameValueCollection requestHeadersToAdd)
        {
        }
    }
}