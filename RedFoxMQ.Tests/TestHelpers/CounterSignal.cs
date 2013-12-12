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
using System.Threading;

// ReSharper disable once CheckNamespace
namespace RedFoxMQ.Tests
{
    class CounterSignal
    {
        private int _counter;
        private readonly int _signalAt;

        private readonly ManualResetEventSlim _counterSignal = new ManualResetEventSlim();

        public CounterSignal(int signalAt = 1, int initialCounterValue = 0)
        {
            _counter = initialCounterValue;
            _signalAt = signalAt;

            SignalIfNeeded(_counter);
        }

        public void Increment()
        {
            var oldValue = Interlocked.Increment(ref _counter);
            SignalIfNeeded(oldValue);
        }

        public void Decrement()
        {
            var oldValue = Interlocked.Decrement(ref _counter);
            SignalIfNeeded(oldValue);
        }

        public bool Wait(TimeSpan timeout)
        {
            return _counterSignal.Wait(timeout);
        }

        private void SignalIfNeeded(int currentValue)
        {
            if (currentValue == _signalAt) _counterSignal.Set();
            else _counterSignal.Reset();
        }
    }
}
