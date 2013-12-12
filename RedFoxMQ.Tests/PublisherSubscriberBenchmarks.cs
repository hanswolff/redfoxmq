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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RedFoxMQ.Tests
{
    [Explicit]
    class PublisherSubscriberBenchmarks
    {
        private const int Count = 20000;
        private readonly static TimeSpan TimeOut = TimeSpan.FromSeconds(30);

        [Test]
        public void One_Publisher_One_Subscriber_Single_Broadcasts()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);
                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Publisher_One_Subscriber_Batch_Broadcast()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);
                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < Count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Publisher_Two_Subscribers_Single_Broadcasts()
        {
            using (var publisher = new Publisher())
            using (var subscriber1 = new Subscriber())
            using (var subscriber2 = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);
                subscriber1.Connect(endpoint);
                subscriber2.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber2.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Publisher_Two_Subscribers_Batch_Broadcast()
        {
            using (var publisher = new Publisher())
            using (var subscriber1 = new Subscriber())
            using (var subscriber2 = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);
                subscriber1.Connect(endpoint);
                subscriber2.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber2.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < Count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Publisher_Ten_Subscribers_Single_Broadcasts()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);

                var subscribers = Enumerable.Range(1, 9).Select(i =>
                {
                    var sub = new Subscriber();
                    sub.Connect(endpoint);

                    return sub;
                }).ToList();

                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Count; i++)
                {
                    publisher.Broadcast(messageSent);
                }
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                subscribers.ForEach(sub => sub.Dispose());

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Publisher_Ten_Subscribers_Batch_Broadcast()
        {
            using (var publisher = new Publisher())
            using (var subscriber = new Subscriber())
            {
                var endpoint = TestHelpers.TcpTestEndpoint;
                publisher.Bind(endpoint);

                var subscribers = Enumerable.Range(1, 9).Select(i =>
                {
                    var sub = new Subscriber();
                    sub.Connect(endpoint);

                    return sub;
                }).ToList();

                subscriber.Connect(endpoint);

                Thread.Sleep(30);

                var counterSignal = new CounterSignal(Count);
                subscriber.MessageReceived += m => counterSignal.Increment();

                var messageSent = new TestMessage { Text = "Hello" };

                var batch = new List<TestMessage>();
                for (var i = 0; i < Count; i++)
                    batch.Add(messageSent);

                var sw = Stopwatch.StartNew();
                publisher.Broadcast(batch);
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                subscribers.ForEach(sub => sub.Dispose());

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:F0} per second)", sw.Elapsed, Count, Count / sw.Elapsed.TotalSeconds);
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
