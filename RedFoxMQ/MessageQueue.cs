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
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueue : IDisposable
    {
        private readonly MessageQueueProcessor _messageQueueProcessor;
        private readonly MessageFrameSender _sender;
        private readonly BlockingCollection<MessageFrame> _singleMessageFrames = new BlockingCollection<MessageFrame>();
        private readonly BlockingCollection<List<MessageFrame>> _batchMessageFrames = new BlockingCollection<List<MessageFrame>>();

        public event Action<IReadOnlyCollection<MessageFrame>> MessageFramesAdded = m => { };

        public MessageQueue(MessageQueueProcessor messageQueueProcessor, MessageFrameSender sender)
        {
            if (messageQueueProcessor == null) throw new ArgumentNullException("messageQueueProcessor");
            if (sender == null) throw new ArgumentNullException("sender");
            _sender = sender;

            _messageQueueProcessor = messageQueueProcessor;
            messageQueueProcessor.Register(this);
        }

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

        internal async Task<bool> SendFromQueue(CancellationToken cancellationToken)
        {
            List<MessageFrame> batch;
            if (!_batchMessageFrames.TryTake(out batch))
            {
                return await SendSingleMessageFrameFromQueue(cancellationToken);
            }

            MessageFrame messageFrame;
            if (_singleMessageFrames.TryTake(out messageFrame)) batch.Add(messageFrame);

            await _sender.SendAsync(batch, cancellationToken);
            return true;
        }

        private async Task<bool> SendSingleMessageFrameFromQueue(CancellationToken cancellationToken)
        {
            MessageFrame messageFrame;
            if (!_singleMessageFrames.TryTake(out messageFrame)) return false;
            await _sender.SendAsync(messageFrame, cancellationToken);
            return true;
        }

        #region Dispose
        private bool _disposed;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    _messageQueueProcessor.Unregister(this);

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~MessageQueue()
        {
            Dispose(false);
        }
        #endregion

    }
}
