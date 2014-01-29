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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueueBatch
    {
        private readonly int _sendBufferSize;

        private readonly BlockingCollection<MessageFrame> _singleMessageFrames = new BlockingCollection<MessageFrame>();
        private readonly BlockingCollection<List<MessageFrame>> _batchMessageFrames = new BlockingCollection<List<MessageFrame>>();
        public readonly CounterSignal MessageCounterSignal = new CounterSignal(1, 0);

        public event Action<IReadOnlyCollection<MessageFrame>> MessageFramesAdded = m => { };

        public int Count { get { return _singleMessageFrames.Count + _batchMessageFrames.Sum(x => x.Count); } }

        public MessageQueueBatch(int sendBufferSize)
        {
            _sendBufferSize = sendBufferSize;
        }

        public void Add(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (_singleMessageFrames.TryAdd(messageFrame))
            {
                MessageCounterSignal.Increment();
                MessageFramesAdded(new [] { messageFrame });
            }
        }

        public void AddRange(IEnumerable<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;

            var batch = new List<MessageFrame>(messageFrames);
            if (_batchMessageFrames.TryAdd(batch))
            {
                MessageCounterSignal.Add(batch.Count);
                MessageFramesAdded(new ReadOnlyCollection<MessageFrame>(batch));
            }
        }

        internal bool SendMultipleFromQueue(IMessageFrameWriter writer)
        {
            List<MessageFrame> batch;
            if (!_batchMessageFrames.TryTake(out batch))
            {
                batch = new List<MessageFrame>();
            }
            else MessageCounterSignal.Add(-batch.Count);

            MessageFrame messageFrame;
            if (_singleMessageFrames.TryTake(out messageFrame))
            {
                MessageCounterSignal.Decrement();

                var batchSize = batch.Sum(x => x.RawMessage.LongLength);
                batch.Add(messageFrame);
                batchSize += messageFrame.RawMessage.LongLength;

                while (batchSize < _sendBufferSize && _singleMessageFrames.TryTake(out messageFrame))
                {
                    batch.Add(messageFrame);
                    batchSize += messageFrame.RawMessage.LongLength;
                }
            }

            writer.WriteMessageFrames(batch);
            return true;
        }

        internal async Task<bool> SendMultipleFromQueueAsync(IMessageFrameWriter writer, CancellationToken cancellationToken)
        {
            List<MessageFrame> batch;
            if (!_batchMessageFrames.TryTake(out batch))
            {
                batch = new List<MessageFrame>();
            }
            else MessageCounterSignal.Add(-batch.Count);

            MessageFrame messageFrame;
            if (_singleMessageFrames.TryTake(out messageFrame))
            {
                MessageCounterSignal.Decrement();

                var batchSize = batch.Sum(x => x.RawMessage.LongLength);
                batch.Add(messageFrame);
                batchSize += messageFrame.RawMessage.LongLength;

                while (batchSize < _sendBufferSize && _singleMessageFrames.TryTake(out messageFrame))
                {
                    batch.Add(messageFrame);
                    batchSize += messageFrame.RawMessage.LongLength;
                }
            }

            await writer.WriteMessageFramesAsync(batch, cancellationToken);
            return true;
        }
    }
}
