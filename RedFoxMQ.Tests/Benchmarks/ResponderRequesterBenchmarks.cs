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
using RedFoxMQ.Transports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests.Benchmarks
{
    public abstract class ResponderRequesterBenchmarks
    {
        private const int NumberOfRequests = 100000;

        public abstract RedFoxEndpoint GetEndpoint();

        [Test]
        public void One_Responder_1_Requester()
        {
            var echoWorker = new ResponderWorker();
            var workerFactory = new ResponderWorkerFactory(request => echoWorker);
            using (var responder = new Responder(workerFactory))
            using (var requester = new Requester())
            {
                var endpoint = GetEndpoint();
                responder.Bind(endpoint);
                requester.Connect(endpoint);

                Thread.Sleep(100);

                var sw = Stopwatch.StartNew();
                var messageSent = new TestMessage();
                for (var i = 0; i < NumberOfRequests; i++)
                {
                    requester.Request(messageSent);
                }
                sw.Stop();

                Assert.Inconclusive("{0} elapsed sending/receiving {1} messages ({2:N0} per second)", sw.Elapsed, NumberOfRequests, NumberOfRequests / sw.Elapsed.TotalSeconds);
            }
        }

        [Test]
        public void One_Responder_2_Requesters()
        {
            One_Responder_N_Requesters(2);
        }

        [Test]
        public void One_Responder_4_Requesters()
        {
            One_Responder_N_Requesters(4);
        }

        [Test]
        public void One_Responder_6_Requesters()
        {
            One_Responder_N_Requesters(6);
        }

        [Test]
        public void One_Responder_8_Requesters()
        {
            One_Responder_N_Requesters(8);
        }

        private void One_Responder_N_Requesters(int n)
        {
            var echoWorker = new ResponderWorker();
            var workerFactory = new ResponderWorkerFactory(request => echoWorker);
            using (var responder = new Responder(workerFactory))
            {
                var endpoint = GetEndpoint();
                responder.Bind(endpoint);

                var requesters = Enumerable.Range(0, n).Select(x => new Requester()).ToList();
                foreach (var requester in requesters)
                {
                    requester.Connect(endpoint);
                }

                Thread.Sleep(100);

                var threadsStartedSignal = new CounterSignal(n, 0);
                var startSignal = new ManualResetEventSlim();
                var tasks = new List<Task>();
                foreach (var requester in requesters)
                {
                    var req = requester;
                    var task = Task.Factory.StartNew(() =>
                    {
                        threadsStartedSignal.Increment();
                        startSignal.Wait();

                        var message = new TestMessage();
                        for (var i = 0; i < NumberOfRequests / n; i++)
                        {
                            req.Request(message);
                        }
                    }, TaskCreationOptions.LongRunning);
                    tasks.Add(task);
                }
                Assert.IsTrue(threadsStartedSignal.Wait(TimeSpan.FromSeconds(1)));

                var sw = Stopwatch.StartNew();
                startSignal.Set();
                Assert.IsTrue(Task.WhenAll(tasks).Wait(TimeSpan.FromMinutes(1)));
                sw.Stop();

                foreach (var requester in requesters)
                {
                    requester.Disconnect();
                }

                Assert.Inconclusive("{0} elapsed sending/receiving {1} messages ({2:N0} per second)", sw.Elapsed, NumberOfRequests, NumberOfRequests / sw.Elapsed.TotalSeconds);
            }
        }

        [SetUp]
        public void Setup()
        {
            TestHelpers.InitializeMessageSerialization();
        }
    }
}
