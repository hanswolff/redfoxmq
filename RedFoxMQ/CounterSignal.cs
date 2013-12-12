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

namespace RedFoxMQ
{
    class CounterSignal
    {
        private int _counter;
        private readonly int _signalGreaterOrEqual;

        private readonly ManualResetEventSlim _counterSignal = new ManualResetEventSlim();

        public bool IsSet
        {
            get { return _counterSignal.IsSet; }
        }

        public int CurrentValue
        {
            get { return _counter; }
            set { _counter = value; }
        }

        public CounterSignal(int signalGreaterOrEqual)
            : this(signalGreaterOrEqual, 0)
        {
        }

        public CounterSignal(int signalGreaterOrEqual, int initialCounterValue)
        {
            _counter = initialCounterValue;
            _signalGreaterOrEqual = signalGreaterOrEqual;

            SignalIfNeeded(_counter);
        }

        public int Add(int value)
        {
            var oldValue = Interlocked.Add(ref _counter, value);
            SignalIfNeeded(oldValue);
            return oldValue;
        }

        public int Increment()
        {
            var oldValue = Interlocked.Increment(ref _counter);
            SignalIfNeeded(oldValue);
            return oldValue;
        }

        public int Decrement()
        {
            var oldValue = Interlocked.Decrement(ref _counter);
            SignalIfNeeded(oldValue);
            return oldValue;
        }

        public bool Wait()
        {
            return Wait(TimeSpan.FromMilliseconds(-1));
        }

        public bool Wait(TimeSpan timeout)
        {
            return _counterSignal.Wait(timeout);
        }

        private void SignalIfNeeded(int currentValue)
        {
            if (currentValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
        }
    }
}
