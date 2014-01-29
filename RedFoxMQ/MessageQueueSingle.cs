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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueueSingle
    {
        private readonly BlockingCollection<MessageFrame> _singleMessageFrames = new BlockingCollection<MessageFrame>();
        public readonly CounterSignal MessageCounterSignal = new CounterSignal(1, 0);

        public event Action<MessageFrame> MessageFrameAdded = m => { };

        public int Count { get { return _singleMessageFrames.Count; } }

        public void Add(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (_singleMessageFrames.TryAdd(messageFrame))
            {
                MessageCounterSignal.Increment();
                MessageFrameAdded(messageFrame);
            }
        }

        public void AddRange(IEnumerable<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;

            foreach (var messageFrame in messageFrames)
            {
                if (_singleMessageFrames.TryAdd(messageFrame))
                {
                    MessageCounterSignal.Increment();
                    MessageFrameAdded(messageFrame);
                }
            }
        }

        internal bool SendFromQueue(IMessageFrameWriter writer)
        {
            MessageFrame messageFrame;
            if (!_singleMessageFrames.TryTake(out messageFrame)) return false;

            MessageCounterSignal.Decrement();
            writer.WriteMessageFrame(messageFrame);
            return true;
        }

        internal async Task<bool> SendMultipleFromQueueAsync(IMessageFrameWriter writer, CancellationToken cancellationToken)
        {
            MessageFrame messageFrame;
            if (!_singleMessageFrames.TryTake(out messageFrame)) return false;

            MessageCounterSignal.Decrement();
            await writer.WriteMessageFrameAsync(messageFrame, cancellationToken);
            return true;
        }
    }
}
