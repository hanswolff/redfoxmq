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
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueueDistributor
    {
        private readonly MessageQueue _messageQueue;
        private readonly ConcurrentDictionary<IMessageFrameWriter, MessageFrameWriterPayload> _messageFrameWriters = new ConcurrentDictionary<IMessageFrameWriter, MessageFrameWriterPayload>();
        private Task _asyncSchedulerTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public MessageQueueDistributor(MessageQueue messageQueue)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            _messageQueue = messageQueue;
        }

        public void Register(IMessageFrameWriter messageFrameWriter)
        {
            if (messageFrameWriter == null) throw new ArgumentNullException("messageFrameWriter");

            var messageQueuePayload = new MessageFrameWriterPayload(messageFrameWriter);
            _messageFrameWriters[messageFrameWriter] = messageQueuePayload;
            StartAsyncProcessingIfNotStartedYet();
        }

        public bool Unregister(IMessageFrameWriter messageFrameWriter)
        {
            if (messageFrameWriter == null) throw new ArgumentNullException("messageFrameWriter");

            MessageFrameWriterPayload messageFrameWriterPayload;
            var removed = _messageFrameWriters.TryRemove(messageFrameWriter, out messageFrameWriterPayload);
            if (removed)
            {
                messageFrameWriterPayload.Cancelled.Set(true);
            }

            if (_messageFrameWriters.IsEmpty)
            {
                StopProcessing();
            }

            return removed;
        }

        private readonly InterlockedBoolean _started = new InterlockedBoolean();
        private void StartAsyncProcessingIfNotStartedYet()
        {
            if (_started.Set(true)) return;

            try
            {
                _cts = new CancellationTokenSource();
                _asyncSchedulerTask = Task.Factory.StartNew(() => ExecuteAsyncLoop(_cts.Token), TaskCreationOptions.LongRunning);
            }
            catch (Exception)
            {
                _started.Set(false);
                throw;
            }
        }

        private void ExecuteAsyncLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    LoopUsingAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _started.Set(false);
            }
        }

        private void LoopUsingAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _messageFrameWriters)
            {
                var messageQueuePayload = item.Value;

                if (messageQueuePayload.Busy.Set(true)) continue;
                var task = LoopMessageQueueAsync(messageQueuePayload, cancellationToken).ConfigureAwait(false);
                return;
            }
        }

        private async Task LoopMessageQueueAsync(MessageFrameWriterPayload messageFrameWriterPayload, CancellationToken cancellationToken)
        {
            try
            {
                await _messageQueue.SendFromQueueAsync(messageFrameWriterPayload.Writer, cancellationToken);
            }
            finally
            {
                messageFrameWriterPayload.Busy.Set(false);
            }
        }

        private void StopProcessing()
        {
            var task = Interlocked.Exchange(ref _asyncSchedulerTask, null);
            if (task == null) return;

            _cts.Cancel(false);

            _started.Set(false);
        }

        struct MessageFrameWriterPayload
        {
            public readonly IMessageFrameWriter Writer;
            public readonly InterlockedBoolean Busy;
            public readonly InterlockedBoolean Cancelled;

            public MessageFrameWriterPayload(IMessageFrameWriter writer)
            {
                Writer = writer;
                Busy = new InterlockedBoolean();
                Cancelled = new InterlockedBoolean();
            }
        }
    }
}
