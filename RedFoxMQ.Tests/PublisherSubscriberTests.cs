using NUnit.Framework;
using RedFoxMQ.Tests.TestHelpers;
using RedFoxMQ.Transports;
using System.Collections.Generic;
using System.Diagnostics;
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
