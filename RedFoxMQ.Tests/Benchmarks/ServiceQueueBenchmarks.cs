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
using System.Diagnostics;

namespace RedFoxMQ.Tests.Benchmarks
{
    public abstract class ServiceQueueBenchmarks
    {
        private const int NumberOfMessages = 100000;
        private readonly static TimeSpan TimeOut = TimeSpan.FromSeconds(120);

        public abstract RedFoxEndpoint GetEndpoint();

        [TestCase(ServiceQueueRotationAlgorithm.FirstIdle)]
        [TestCase(ServiceQueueRotationAlgorithm.LoadBalance)]
        public void ServiceQueue_1_Writer_1_Reader(ServiceQueueRotationAlgorithm algorithm)
        {
            using (var serviceQueue = new ServiceQueue(algorithm))
            using (var reader = new ServiceQueueReader())
            using (var writer = new ServiceQueueWriter())
            {
                var endpoint = GetEndpoint();
                serviceQueue.Bind(endpoint);

                var counterSignal = new CounterSignal(NumberOfMessages, 0);
                reader.MessageReceived += (s, m) => counterSignal.Increment();

                reader.Connect(endpoint);
                writer.Connect(endpoint);

                var messageSent = new TestMessage();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < NumberOfMessages; i++)
                {
                    writer.SendMessage(messageSent);
                }
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:N0} per second)", sw.Elapsed, NumberOfMessages, NumberOfMessages / sw.Elapsed.TotalSeconds);

            }
        }

        [TestCase(ServiceQueueRotationAlgorithm.FirstIdle)]
        [TestCase(ServiceQueueRotationAlgorithm.LoadBalance)]
        public void ServiceQueue_1_Writer_2_Readers(ServiceQueueRotationAlgorithm algorithm)
        {
            ServiceQueue_1_Writer_n_Readers(algorithm, 2);
        }

        [TestCase(ServiceQueueRotationAlgorithm.FirstIdle)]
        [TestCase(ServiceQueueRotationAlgorithm.LoadBalance)]
        public void ServiceQueue_1_Writer_4_Readers(ServiceQueueRotationAlgorithm algorithm)
        {
            ServiceQueue_1_Writer_n_Readers(algorithm, 4);
        }

        [TestCase(ServiceQueueRotationAlgorithm.FirstIdle)]
        [TestCase(ServiceQueueRotationAlgorithm.LoadBalance)]
        public void ServiceQueue_1_Writer_8_Readers(ServiceQueueRotationAlgorithm algorithm)
        {
            ServiceQueue_1_Writer_n_Readers(algorithm, 8);
        }

        private void ServiceQueue_1_Writer_n_Readers(ServiceQueueRotationAlgorithm algorithm, int readers)
        {
            using (var serviceQueue = new ServiceQueue(algorithm))
            using (var writer = new ServiceQueueWriter())
            {
                var endpoint = GetEndpoint();
                serviceQueue.Bind(endpoint);

                var counterSignal = new CounterSignal(NumberOfMessages, 0);

                for (var i = 0; i < readers; i++)
                {
                    var reader = new ServiceQueueReader();
                    reader.MessageReceived += (s, m) => counterSignal.Increment();

                    reader.Connect(endpoint);
                }

                writer.Connect(endpoint);

                var messageSent = new TestMessage();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < NumberOfMessages; i++)
                {
                    writer.SendMessage(messageSent);
                }
                Assert.IsTrue(counterSignal.Wait(TimeOut), "Timeout waiting for message");
                sw.Stop();

                Assert.Inconclusive("{0} elapsed reading {1} messages ({2:N0} per second)", sw.Elapsed, NumberOfMessages, NumberOfMessages / sw.Elapsed.TotalSeconds);

            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
