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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class RequestResponderTests
    {
        public static readonly TimeSpan Timeout = !Debugger.IsAttached ? TimeSpan.FromSeconds(10) : TimeSpan.FromMilliseconds(-1);

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Request_Response_single_message(RedFoxTransport transport)
        {
            using (var responder = TestHelpers.CreateTestResponder())
            using (var requester = new Requester())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);

                requester.Connect(endpoint);

                Thread.Sleep(100);

                var cts = new CancellationTokenSource(Timeout);
                var messageSent = new TestMessage { Text = "Hello" };
                var messageReceived = (TestMessage)requester.Request(messageSent, cts.Token);

                Assert.AreEqual(messageSent.Text, messageReceived.Text);
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Request_Response_two_messages(RedFoxTransport transport)
        {
            using (var responder = TestHelpers.CreateTestResponder())
            using (var requester = new Requester())
            {
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);
                requester.Connect(endpoint);

                var messagesReceived = new List<TestMessage>();
                Thread.Sleep(100);

                var messageSent = new TestMessage { Text = "Hello" };
                var cts = new CancellationTokenSource(Timeout);
                messagesReceived.Add((TestMessage)requester.Request(messageSent, cts.Token));
                messagesReceived.Add((TestMessage)requester.Request(messageSent, cts.Token));

                Assert.AreEqual(messageSent.Text, messagesReceived[0].Text);
                Assert.AreEqual(messageSent.Text, messagesReceived[1].Text);
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Request_Response_different_threads_large_message(RedFoxTransport transport)
        {
            var endpoint = TestHelpers.CreateEndpointForTransport(transport);
            var started = new ManualResetEventSlim();
            var stop = new ManualResetEventSlim();

            Task.Run(() =>
            {
                using (var responder = TestHelpers.CreateTestResponder())
                {
                    responder.Bind(endpoint);
                    started.Set();
                    stop.Wait();
                }
            });

            var largeMessage = new TestMessage { Text = new string('x', 1024 * 1024) };

            TestMessage messageReceived = null;
            var signal = new ManualResetEventSlim();
            Task.Run(() =>
            {
                using (var requester = new Requester())
                {
                    started.Wait(Timeout);

                    requester.Connect(endpoint);

                    messageReceived = (TestMessage)requester.Request(largeMessage);

                    stop.Wait();
                }
            });

            try
            {
                Assert.IsTrue(signal.Wait(Timeout));
                Assert.AreEqual(largeMessage.Text, messageReceived.Text);
            }
            finally
            {
                stop.Set();
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Responder_ClientConnected_event_fired(RedFoxTransport transport)
        {
            using (var responder = TestHelpers.CreateTestResponder())
            using (var requester = new Requester())
            {
                var eventFired = new ManualResetEventSlim();
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.ClientConnected += s => eventFired.Set();
                responder.Bind(endpoint);

                requester.Connect(endpoint);

                Thread.Sleep(100);

                requester.Disconnect();

                Assert.IsTrue(eventFired.Wait(Timeout));
            }
        }

        [TestCase(RedFoxTransport.Inproc)]
        [TestCase(RedFoxTransport.Tcp)]
        public void Requester_Disconnected_event_fired(RedFoxTransport transport)
        {
            using (var responder = TestHelpers.CreateTestResponder())
            using (var requester = new Requester())
            {
                var eventFired = new ManualResetEventSlim();
                var endpoint = TestHelpers.CreateEndpointForTransport(transport);

                responder.Bind(endpoint);

                requester.Disconnected += eventFired.Set;
                requester.Connect(endpoint);

                Thread.Sleep(100);

                requester.Disconnect();

                Assert.IsTrue(eventFired.Wait(Timeout));
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
