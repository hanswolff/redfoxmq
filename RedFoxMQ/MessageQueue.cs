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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueue
    {
        private readonly BlockingCollection<MessageFrame> _singleMessageFrames = new BlockingCollection<MessageFrame>();
        private readonly BlockingCollection<List<MessageFrame>> _batchMessageFrames = new BlockingCollection<List<MessageFrame>>();

        public event Action<IReadOnlyCollection<MessageFrame>> MessageFramesAdded = m => { };

        public void Add(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (_singleMessageFrames.TryAdd(messageFrame))
            {
                MessageFramesAdded(new [] { messageFrame });
            }
        }

        public void AddRange(IEnumerable<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;

            var batch = new List<MessageFrame>(messageFrames);
            if (_batchMessageFrames.TryAdd(batch))
            {
                MessageFramesAdded(new ReadOnlyCollection<MessageFrame>(batch));
            }
        }

        private const int TryMakeBatchSizeLargerThan = 65536;
        internal bool SendFromQueue(MessageFrameSender sender)
        {
            List<MessageFrame> batch;
            if (!_batchMessageFrames.TryTake(out batch))
            {
                batch = new List<MessageFrame>();
            }

            MessageFrame messageFrame;
            if (_singleMessageFrames.TryTake(out messageFrame))
            {
                var batchSize = batch.Sum(x => x.RawMessage.LongLength);
                batch.Add(messageFrame);
                batchSize += messageFrame.RawMessage.LongLength;

                while (batchSize < TryMakeBatchSizeLargerThan && _singleMessageFrames.TryTake(out messageFrame))
                {
                    batch.Add(messageFrame);
                    batchSize += messageFrame.RawMessage.LongLength;
                }
            }

            sender.Send(batch);
            return true;
        }

        internal async Task<bool> SendFromQueueAsync(MessageFrameSender sender, CancellationToken cancellationToken)
        {
            List<MessageFrame> batch;
            if (!_batchMessageFrames.TryTake(out batch))
            {
                batch = new List<MessageFrame>();
            }

            MessageFrame messageFrame;
            if (_singleMessageFrames.TryTake(out messageFrame))
            {
                var batchSize = batch.Sum(x => x.RawMessage.LongLength);
                batch.Add(messageFrame);
                batchSize += messageFrame.RawMessage.LongLength;

                while (batchSize < TryMakeBatchSizeLargerThan && _singleMessageFrames.TryTake(out messageFrame))
                {
                    batch.Add(messageFrame);
                    batchSize += messageFrame.RawMessage.LongLength;
                }
            }

            await sender.SendAsync(batch, cancellationToken);
            return true;
        }
    }
}
