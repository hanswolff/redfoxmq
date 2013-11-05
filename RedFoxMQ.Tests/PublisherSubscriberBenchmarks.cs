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
                subscriber.ConnectAsync(endpoint).Wait();

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
                subscriber1.ConnectAsync(endpoint).Wait();
                subscriber2.ConnectAsync(endpoint).Wait();

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
                    sub.ConnectAsync(endpoint).Wait();

                    return sub;
                }).ToList();

                subscriber.ConnectAsync(endpoint).Wait();

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
