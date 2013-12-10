// 
// Copyright 2013 Hans Wolff
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using NUnit.Framework;
using RedFoxMQ.Transports;
using System;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ResponderTests
    {
        [Test]
        public void responders_bind_same_endpoint_twice_fails()
        {
            using (var responder = TestHelpers.CreateTestResponder())
            {
                var endpoint = new RedFoxEndpoint("/path");
                responder.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => responder.Bind(endpoint));
            }
        }

        [Test]
        public void responder_can_bind_multiple_different_endpoints()
        {
            using (var responder = TestHelpers.CreateTestResponder())
            {
                responder.Bind(new RedFoxEndpoint("/path1"));
                responder.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void responder_dispose_unbinds_endpoints()
        {
            using (var responder = TestHelpers.CreateTestResponder())
            {
                responder.Bind(new RedFoxEndpoint("/path1"));
                responder.Bind(new RedFoxEndpoint("/path2"));
            }

            using (var responder = TestHelpers.CreateTestResponder())
            {
                responder.Bind(new RedFoxEndpoint("/path1"));
                responder.Bind(new RedFoxEndpoint("/path2"));
            }
        }

        [Test]
        public void two_responders_same_endpoint_fails()
        {
            using (var responder1 = TestHelpers.CreateTestResponder())
            using (var responder2 = TestHelpers.CreateTestResponder())
            {
                var endpoint = new RedFoxEndpoint("/path");
                responder1.Bind(endpoint);
                Assert.Throws<InvalidOperationException>(() => responder2.Bind(endpoint));
            }
        }
    }
}
