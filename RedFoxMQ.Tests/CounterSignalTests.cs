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
using System;
using System.Diagnostics;
using NUnit.Framework;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class CounterSignalTests
    {
        [Test]
        public void initial_value_signal_set()
        {
            var counterSignal = new CounterSignal(1, 1);
            Assert.IsTrue(counterSignal.IsSet);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void initial_value_signal_not_set()
        {
            var counterSignal = new CounterSignal(2, 1);
            Assert.IsFalse(counterSignal.IsSet);
            Assert.IsFalse(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_increment_signal_set()
        {
            var counterSignal = new CounterSignal(2, 1);
            counterSignal.Increment();
            Assert.IsTrue(counterSignal.IsSet);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_add_signal_set()
        {
            var counterSignal = new CounterSignal(10, 1);
            counterSignal.Add(100);
            Assert.IsTrue(counterSignal.IsSet);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_add_signal_not_set()
        {
            var counterSignal = new CounterSignal(10, 0);
            counterSignal.Add(9);
            Assert.IsFalse(counterSignal.IsSet);
            Assert.IsFalse(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_increment_then_decrement_signal_not_set()
        {
            var counterSignal = new CounterSignal(2, 1);
            counterSignal.Increment();
            counterSignal.Decrement();
            Assert.IsFalse(counterSignal.IsSet);
            Assert.IsFalse(counterSignal.Wait(TimeSpan.Zero));
        }

        [Explicit]
        [Test]
        public void benchmark_get_value()
        {
            var counterSignal = new CounterSignal();

            const int iterations = 10000000;
            var value = false;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                value |= counterSignal.Wait(TimeSpan.Zero);
            }
            sw.Stop();

            if (value) Console.WriteLine(); // prevent too aggressive optimization

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }

        [Explicit]
        [Test]
        public void benchmark_increment_value()
        {
            var counterSignal = new CounterSignal();

            const int iterations = 10000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                counterSignal.Increment();
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }
    }
}
