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
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ServiceQueueReaderTests
    {
        [Test]
        public void ServiceQueue_connect_AddMessageFrame_single_message_received()
        {
            using (var serviceQueue = new ServiceQueue())
            using (var reader = new ServiceQueueReader())
            {
                var endpoint = new RedFoxEndpoint("/path");

                IMessage messageReceived = null;
                var received = new ManualResetEventSlim();
                reader.MessageReceived += m =>
                {
                    messageReceived = m;
                    received.Set();
                };

                serviceQueue.Bind(endpoint);
                reader.Connect(endpoint);

                var testMessage = new TestMessage();
                var testMessageFrame = new MessageFrameCreator(DefaultMessageSerialization.Instance).CreateFromMessage(testMessage);

                serviceQueue.AddMessageFrame(testMessageFrame);

                Assert.IsTrue(received.Wait(TimeSpan.FromSeconds(1)));
                Assert.AreEqual(testMessage, messageReceived);
            }
        }

        [Test]
        public void ServiceQueue_AddMessageFrame_connect_single_message_received()
        {
            using (var serviceQueue = new ServiceQueue())
            using (var reader = new ServiceQueueReader())
            {
                var endpoint = new RedFoxEndpoint("/path");

                IMessage messageReceived = null;
                var received = new ManualResetEventSlim();
                reader.MessageReceived += m =>
                {
                    messageReceived = m;
                    received.Set();
                };

                var testMessage = new TestMessage();
                var testMessageFrame = new MessageFrameCreator(DefaultMessageSerialization.Instance).CreateFromMessage(testMessage);

                serviceQueue.AddMessageFrame(testMessageFrame);

                serviceQueue.Bind(endpoint);
                reader.Connect(endpoint);

                Assert.IsTrue(received.Wait(TimeSpan.FromSeconds(1)));
                Assert.AreEqual(testMessage, messageReceived);
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
