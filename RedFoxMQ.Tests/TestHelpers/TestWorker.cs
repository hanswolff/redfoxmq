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
    class TestWorker : IResponderWorker
    {
        private readonly int _sleepDelay;
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _completed = new ManualResetEventSlim();

        public TestWorker(int sleepDelay)
        {
            _sleepDelay = sleepDelay;
        }

        public IMessage GetResponse(IMessage requestMessage, object state)
        {
            _started.Set();
            Thread.Sleep(_sleepDelay);
            _completed.Set();
            return requestMessage;
        }

        public bool WaitStarted()
        {
            return WaitStarted(TimeSpan.FromMilliseconds(-1));
        }

        public bool WaitStarted(TimeSpan timeout)
        {
            return _started.Wait(timeout);
        }

        public bool WaitCompleted()
        {
            return WaitCompleted(TimeSpan.FromMilliseconds(-1));
        }

        public bool WaitCompleted(TimeSpan timeout)
        {
            return _completed.Wait(timeout);
        }
    }
}
