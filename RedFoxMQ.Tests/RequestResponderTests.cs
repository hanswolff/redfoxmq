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
using RedFoxMQ.Tests.TestHelpers;
using RedFoxMQ.Transports;
using System.Collections.Generic;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class RequestResponderTests
    {
        [Test]
        public void Request_Response_single_message()
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                responder.Bind(endpoint);
                requester.ConnectAsync(endpoint).Wait();

                var messageSent = new TestMessage { Text = "Hello" };
                var messageReceived = (TestMessage)requester.Request(messageSent).Result;

                Assert.AreEqual(messageSent.Text, messageReceived.Text);
            }
        }

        [Test]
        public void Request_Response_two_messages()
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

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

        [SetUp]
        public void Setup()
        {
            MessageSerialization.Instance.RegisterSerializer(new TestMessage().MessageTypeId, new TestMessageSerializer());
            MessageSerialization.Instance.RegisterDeserializer(new TestMessage().MessageTypeId, new TestMessageDeserializer());
        }
    }
}
