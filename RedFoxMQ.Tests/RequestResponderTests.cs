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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class RequestResponderTests
    {
        public static readonly int TestTimeoutInMillis = Debugger.IsAttached ? -1 : 10000;

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Request_Response_single_message(RedFoxTransport transport)
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);
                requester.ConnectAsync(endpoint).Wait();

                var messageSent = new TestMessage { Text = "Hello" };
                var messageReceived = (TestMessage)requester.Request(messageSent).Result;

                Assert.AreEqual(messageSent.Text, messageReceived.Text);
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Request_Response_two_messages(RedFoxTransport transport)
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);
                requester.ConnectAsync(endpoint).Wait();

                var messageReceived = new List<TestMessage>();

                var messageSent = new TestMessage { Text = "Hello" };
                messageReceived.Add((TestMessage)requester.Request(messageSent).Result);
                messageReceived.Add((TestMessage)requester.Request(messageSent).Result);

                Assert.AreEqual(messageSent.Text, messageReceived[0].Text);
                Assert.AreEqual(messageSent.Text, messageReceived[1].Text);
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Responder_ClientConnected_event_fired(RedFoxTransport transport)
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var eventFired = new ManualResetEventSlim();
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.ClientConnected += s => eventFired.Set();
                responder.Bind(endpoint);

                requester.ConnectAsync(endpoint).Wait();
                requester.Disconnect();

                Assert.IsTrue(eventFired.Wait(TestTimeoutInMillis));
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Requester_Disconnected_event_fired(RedFoxTransport transport)
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var eventFired = new ManualResetEventSlim();
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);

                requester.Disconnected += eventFired.Set;
                requester.ConnectAsync(endpoint).Wait();
                requester.Disconnect();

                Assert.IsTrue(eventFired.Wait(TestTimeoutInMillis));
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
