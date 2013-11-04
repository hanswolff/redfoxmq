using NUnit.Framework;
using RedFoxMQ.Tests.TestHelpers;
using RedFoxMQ.Transports;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RedFoxMQ.Tests
{
    [Explicit]
    class PublisherSubscriberBenchmarks
    {
        private const int TimeOut = 30000;

        [TestCase(10000)]
        public void One_Publisher_One_Subscriber(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                publisher.Bind(endpoint);

                subscriber.Connect(endpoint);
                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);

                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
            }
        }

        [TestCase(10000)]
        public void One_Publisher_Two_Subscribers(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber1 = new Subscriber())
            using (var subscriber2 = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                publisher.Bind(endpoint);

                subscriber1.Connect(endpoint);
                Thread.Sleep(30);

                subscriber2.Connect(endpoint);
                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber2.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);
                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
            }
        }

        [TestCase(10000)]
        public void One_Publisher_Ten_Subscribers(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                publisher.Bind(endpoint);

                var subscribers = Enumerable.Range(1, 9).Select(i =>
                {
                    var sub = new Subscriber();
                    sub.Connect(endpoint);
                    Thread.Sleep(30);
                    return sub;
                }).ToList();

                subscriber.Connect(endpoint);
                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);
                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                subscribers.ForEach(sub => sub.Dispose());

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
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
