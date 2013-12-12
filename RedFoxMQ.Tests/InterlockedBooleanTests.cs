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
    public class InterlockedBooleanTests
    {
        [Test]
        public void initial_value_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            Assert.IsFalse(interlockedBoolean.Value);
        }

        [Test]
        public void initial_value_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            Assert.IsTrue(interlockedBoolean.Value);
        }

        [Test]
        public void set_value_true_was_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.Set(true);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void set_value_true_was_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.Set(true);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Test]
        public void CompareExchange_true_was_false_compare_with_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.CompareExchange(true, false);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void CompareExchange_true_was_true_compare_with_false()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.CompareExchange(true, false);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Test]
        public void CompareExchange_false_was_false_compare_with_true()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.CompareExchange(false, true);

            Assert.IsFalse(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void CompareExchange_false_was_true_compare_with_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.CompareExchange(false, true);

            Assert.IsFalse(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Test]
        public void benchmark_get_value()
        {
            var interlockedBoolean = new InterlockedBoolean();

            const int iterations = 10000000;
            var value = false;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                value |= interlockedBoolean.Value;
            }
            sw.Stop();

            if (value) Console.WriteLine(); // prevent too aggressive optimization

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }

        [Test]
        public void benchmark_set_value()
        {
            var interlockedBoolean = new InterlockedBoolean();

            const int iterations = 10000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                interlockedBoolean.Set(true);
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }
    }
}
