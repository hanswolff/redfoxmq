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
using RedFoxMQ.Transports.InProc;
using System;
using System.Threading;

namespace RedFoxMQ.Tests.Transports.InProc
{
    [TestFixture]
    public class BlockingConcurrentQueueTests
    {
        [Test]
        public void empty_waits_until_cancelled()
        {
            var queue = new BlockingConcurrentQueue<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

            Assert.Throws<OperationCanceledException>(() => queue.Dequeue(cts.Token));
        }

        [Test]
        public void enqueue_then_does_not_wait_on_dequeue()
        {
            var queue = new BlockingConcurrentQueue<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

            queue.Enqueue(1);
            Assert.AreEqual(1, queue.Dequeue(cts.Token));
        }

        [Test]
        public void enqueue_dequeue_then_wait_on_dequeue()
        {
            var queue = new BlockingConcurrentQueue<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

            queue.Enqueue(1);
            queue.Dequeue(cts.Token);
            Assert.Throws<OperationCanceledException>(() => queue.Dequeue(cts.Token));
        }

        [Test]
        public void enqueue_dequeue_enqueue_does_not_wait_on_dequeue()
        {
            var queue = new BlockingConcurrentQueue<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

            queue.Enqueue(1);
            Assert.AreEqual(1, queue.Dequeue(cts.Token));

            queue.Enqueue(2);
            Assert.AreEqual(2, queue.Dequeue(cts.Token));
        }

        [Test]
        public void enqueue_enqueue_does_not_wait_on_both_dequeue()
        {
            var queue = new BlockingConcurrentQueue<int>();
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

            queue.Enqueue(1);
            queue.Enqueue(2);

            Assert.AreEqual(1, queue.Dequeue(cts.Token));
            Assert.AreEqual(2, queue.Dequeue(cts.Token));
        }
    }
}
