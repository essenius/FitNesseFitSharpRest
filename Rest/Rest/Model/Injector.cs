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

using Rest.ContentObjects;

namespace Rest.Model
{
    /// <summary>
    ///     DIY Dependency injection container, only visible to the program entry points
    ///     (which are the fixture constructors for ContentHandler, ContentObject, RestTester, RestConfig)
    /// </summary>
    internal static class Injector
    {
        private static SessionContext _sessionContext;

        public static ContentObjectFactory InjectContentObjectFactory() =>
            new ContentObjectFactory(InjectSessionContext());

        private static IRestRequestFactory InjectRestRequestFactory() => new RestRequestFactory();

        public static RestSession InjectRestSession(string endPoint) =>
            new RestSession(endPoint, InjectSessionContext(), InjectRestRequestFactory());

        /// <remarks>only one instance</remarks>
        public static SessionContext InjectSessionContext() => _sessionContext ??= new SessionContext();

        /// <remarks>Not for production usage. Testing purposes only</remarks>
        internal static void CleanSessionContext() => _sessionContext = null;
    }
}