// 
// Copyright 2013-2014 Hans Wolff
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
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ServiceQueueWriterTests
    {
        [Test]
        public void ServiceQueue_single_message_sent_MessageFramesCount_is_one()
        {
            using (var serviceQueue = new ServiceQueue())
            using (var writer = new ServiceQueueWriter())
            {
                var endpoint = new RedFoxEndpoint("/path");

                var received = new ManualResetEventSlim();
                serviceQueue.MessageFrameReceived += m => received.Set();

                serviceQueue.Bind(endpoint);
                writer.Connect(endpoint);

                Assert.AreEqual(0, serviceQueue.MessageFramesCount);
                writer.SendMessage(new TestMessage());

                Assert.IsTrue(received.Wait(TimeSpan.FromSeconds(1)));
                Assert.AreEqual(1, serviceQueue.MessageFramesCount);
            }
        }

        [Test]
        public void ServiceQueue_single_message_received_fires_MessageFrameReceived_event()
        {
            using (var serviceQueue = new ServiceQueue())
            using (var writer = new ServiceQueueWriter())
            {
                var endpoint = new RedFoxEndpoint("/path");

                MessageFrame messageFrame = null;
                var received = new ManualResetEventSlim();
                serviceQueue.MessageFrameReceived += m =>
                {
                    messageFrame = m;
                    received.Set();
                };

                serviceQueue.Bind(endpoint);
                writer.Connect(endpoint);

                writer.SendMessage(new TestMessage());

                Assert.IsTrue(received.Wait(TimeSpan.FromSeconds(1)));
                Assert.IsNotNull(messageFrame);
            }
        }

        [Test]
        public void ServiceQueue_multiple_message_received()
        {
            const int count = 1000;

            using (var serviceQueue = new ServiceQueue())
            using (var writer = new ServiceQueueWriter())
            {
                var endpoint = new RedFoxEndpoint("/path");

                var messageFrames = new List<MessageFrame>();
                var counterSignal = new CounterSignal(count, 0);
                serviceQueue.MessageFrameReceived += m =>
                {
                    messageFrames.Add(m);
                    counterSignal.Increment();
                };

                serviceQueue.Bind(endpoint);
                writer.Connect(endpoint);

                for (var i = 0; i < count; i++)
                    writer.SendMessage(new TestMessage());

                Assert.IsTrue(counterSignal.Wait(TimeSpan.FromSeconds(30)));
                Assert.AreEqual(count, messageFrames.Count);
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
