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
using System.Collections.Generic;
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
        public void One_Publisher_One_Subscriber_Single_Broadcasts(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);
                subscriber.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

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
        public void One_Publisher_One_Subscriber_Batch_Broadcast(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);
                subscriber.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);

                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
            }
        }

        [TestCase(10000)]
        public void One_Publisher_Two_Subscribers_Single_Broadcasts(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber1 = new Subscriber())
            using (var subscriber2 = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);
                subscriber1.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();
                subscriber2.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

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
        public void One_Publisher_Two_Subscribers_Batch_Broadcast(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber1 = new Subscriber())
            using (var subscriber2 = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);
                subscriber1.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();
                subscriber2.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber2.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);
                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
            }
        }

        [TestCase(10000)]
        public void One_Publisher_Ten_Subscribers_Single_Broadcasts(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);

                var subscribers = Enumerable.Range(1, 9).Select(i =>
                {
                    var sub = new Subscriber();
                    sub.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

                    return sub;
                }).ToList();

                subscriber.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

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

        [TestCase(10000)]
        public void One_Publisher_Ten_Subscribers_Batch_Broadcast(int count)
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                publisher.Bind(TestHelpers.TcpTestEndpoint);

                var subscribers = Enumerable.Range(1, 9).Select(i =>
                {
                    var sub = new Subscriber();
                    sub.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

                    return sub;
                }).ToList();

                subscriber.ConnectAsync(TestHelpers.TcpTestEndpoint).Wait();

                Thread.Sleep(30);

                var isMessageReceived = new ManualResetEventSlim();

                var receivedCount = 0;
                subscriber.MessageReceived += m =>
                {
                    Interlocked.Increment(ref receivedCount);
                    if (receivedCount >= count) isMessageReceived.Set();
                };

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(isMessageReceived.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                subscribers.ForEach(sub => sub.Dispose());

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, count, count / sw.Elapsed.TotalSeconds);
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
