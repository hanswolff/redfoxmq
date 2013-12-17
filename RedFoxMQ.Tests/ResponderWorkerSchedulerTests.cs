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
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class ResponderWorkerSchedulerTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

        [Test]
        public void ResponderWorkerScheduler_constructor_MinThreads_negative_throws_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResponderWorkerScheduler(-1, 1));
        }

        [Test]
        public void ResponderWorkerScheduler_constructor_MaxThreads_zero_throws_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResponderWorkerScheduler(0, 0));
        }

        [Test]
        public void ResponderWorkerScheduler_constructor_MaxThreads_less_than_MinThreads_throws_ArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResponderWorkerScheduler(1, 0));
        }

        [Test]
        public void ResponderWorkerScheduler_number_of_MinThreads_created()
        {
            using (var scheduler = new ResponderWorkerScheduler(1, 2))
            {
                Assert.AreEqual(1, scheduler.CurrentWorkerThreadCount);
            }
        }

        [Test]
        public void ResponderWorkerScheduler_CurrentBusyThreadCount_increased_when_busy()
        {
            var worker = new TestWorker(30);
            using (var scheduler = new ResponderWorkerScheduler(1, 1))
            {
                Assert.AreEqual(0, scheduler.CurrentBusyThreadCount);
                
                scheduler.AddWorker(worker, new TestMessage(), null);

                worker.WaitStarted();
                Assert.AreEqual(1, scheduler.CurrentBusyThreadCount);
            }
        }

        [Test]
        public void ResponderWorkerScheduler_CurrentBusyThreadCount_decreased_when_back_idle()
        {
            var worker = new TestWorker(30);
            using (var scheduler = new ResponderWorkerScheduler(1, 1))
            {
                scheduler.AddWorker(worker, new TestMessage(), null);

                worker.WaitStarted();
                Assert.AreEqual(1, scheduler.CurrentBusyThreadCount);

                worker.WaitCompleted();
                Thread.Sleep(15);
                Assert.AreEqual(0, scheduler.CurrentBusyThreadCount);
            }
        }

        [Test]
        public void ResponderWorkerScheduler_CurrentBusyThreadCount_increased_then_decreased_then_increased()
        {
            using (var scheduler = new ResponderWorkerScheduler(1, 1))
            {
                var worker1 = new TestWorker(30);
                scheduler.AddWorker(worker1, new TestMessage(), null);

                worker1.WaitStarted();
                Assert.AreEqual(1, scheduler.CurrentBusyThreadCount);

                worker1.WaitCompleted();
                Thread.Sleep(10);
                Assert.AreEqual(0, scheduler.CurrentBusyThreadCount);

                var worker2 = new TestWorker(30);
                scheduler.AddWorker(worker2, new TestMessage(), null);

                worker2.WaitStarted();
                Assert.AreEqual(1, scheduler.CurrentBusyThreadCount);
            }
        }

        [Test]
        public void ResponderWorkerScheduler_CurrentBusyThreadCount_increased_twice()
        {
            using (var scheduler = new ResponderWorkerScheduler(0, 2))
            {
                Assert.AreEqual(0, scheduler.CurrentBusyThreadCount);

                var worker1 = new TestWorker(10);
                scheduler.AddWorker(worker1, new TestMessage(), null);
                worker1.WaitStarted();
                Assert.AreEqual(1, scheduler.CurrentBusyThreadCount);

                var worker2 = new TestWorker(10);
                scheduler.AddWorker(worker2, new TestMessage(), null);
                worker2.WaitStarted();
                Assert.AreEqual(2, scheduler.CurrentBusyThreadCount);
            }
        }
    }
}
