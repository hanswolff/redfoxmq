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

using System.Collections.Concurrent;
using System.Threading;

namespace RedFoxMQ.Transports.InProc
{
    /// <summary>
    /// ConcurrentQueue that blocks on Dequeue if empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockingConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly CounterSignal _counterSignal = new CounterSignal(1, 0);

        /// <summary>
        /// Gets and removes the object at the beginning of the queue
        /// </summary>
        public T Dequeue(CancellationToken cancellationToken)
        {
            T item;
            do
            {
                _counterSignal.Wait(cancellationToken);
            } while (!_queue.TryDequeue(out item));
            _counterSignal.Decrement();
            return item;
        }

        /// <summary>
        /// Adds an object to the end of the queue
        /// </summary>
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _counterSignal.Increment();
        }
    }
}
