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

using System;
using System.Threading;

namespace RedFoxMQ
{
    /// <summary>
    /// Counter that can can be waited on for reaching a certain value
    /// </summary>
    public class CounterSignal
    {
        private long _counter;
        private readonly long _signalGreaterOrEqual;

        private readonly ManualResetEventSlim _counterSignal = new ManualResetEventSlim();

        /// <summary>
        /// Determines if signal is set (meaning CurrentValue >= signalGreaterOrEqual)
        /// </summary>
        public bool IsSet
        {
            get { return _counterSignal.IsSet; }
        }

        /// <summary>
        /// Current counter value
        /// </summary>
        public long CurrentValue
        {
            get { return _counter; }
            set { _counter = value; }
        }

        public CounterSignal(long signalGreaterOrEqual)
            : this(signalGreaterOrEqual, 0)
        {
        }

        public CounterSignal(long signalGreaterOrEqual, long initialCounterValue)
        {
            _counter = initialCounterValue;
            _signalGreaterOrEqual = signalGreaterOrEqual;

            if (initialCounterValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
        }

        /// <summary>
        /// Add a value to counter
        /// </summary>
        /// <returns>new value after adding to the current value</returns>
        public long Add(long value)
        {
            var newValue = Interlocked.Add(ref _counter, value);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset(); 
            return newValue;
        }

        /// <summary>
        /// Increment counter value
        /// </summary>
        /// <returns>new value after incrementing the current value</returns>
        public long Increment()
        {
            var newValue = Interlocked.Increment(ref _counter);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
            return newValue;
        }

        /// <summary>
        /// Decrement counter value
        /// </summary>
        /// <returns>new value after decrementing the current value</returns>
        public long Decrement()
        {
            var newValue = Interlocked.Decrement(ref _counter);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
            return newValue;
        }

        public bool Wait()
        {
            return Wait(TimeSpan.FromMilliseconds(-1));
        }

        public bool Wait(TimeSpan timeout)
        {
            return _counterSignal.Wait(timeout);
        }

        public void Wait(CancellationToken cancellationToken)
        {
            _counterSignal.Wait(cancellationToken);
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _counterSignal.Wait(timeout, cancellationToken);
        }
    }
}
