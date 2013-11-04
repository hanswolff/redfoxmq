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
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class PublisherSubscriberTests
    {
        [Test]
        public void Subscribe_to_Publisher_receive_single_broadcasted_message()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                publisher.Bind(endpoint);
                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();
                TestMessage messageReceived = null;
                subscriber.MessageReceived += m =>
                {
                    messageReceived = (TestMessage)m; 
                    isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                publisher.Broadcast(messageSent);
                Assert.IsTrue(isMessageReceived.Wait(30000), "Timeout waiting for message");

                Assert.AreEqual(messageSent.Text, messageReceived.Text);
            }
        }

        [Test]
        public void Subscribe_to_Publisher_receive_two_broadcasted_messages()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                publisher.Bind(endpoint);
                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();
                var messageReceived = new List<TestMessage>();
                subscriber.MessageReceived += m => { 
                    messageReceived.Add((TestMessage)m); 
                    if (messageReceived.Count == 2) isMessageReceived.Set(); 
                };

                var messageSent = new TestMessage { Text = "Hello" };

                publisher.Broadcast(messageSent);
                publisher.Broadcast(messageSent);

                Assert.IsTrue(isMessageReceived.Wait(30000), "Timeout waiting for messages");

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
