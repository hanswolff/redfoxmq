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
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueueProcessor
    {
        private readonly ConcurrentDictionary<MessageQueue, MessageQueuePayload> _messageQueues = new ConcurrentDictionary<MessageQueue, MessageQueuePayload>();
        private readonly AutoResetEvent _messageQueueHasMessage = new AutoResetEvent(false);
        private Task _asyncSchedulerTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Register(MessageQueue messageQueue, IMessageFrameWriter messageFrameWriter)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            if (messageFrameWriter == null) throw new ArgumentNullException("messageFrameWriter");

            var messageQueuePayload = new MessageQueuePayload(messageFrameWriter);
            messageQueue.MessageFramesAdded += MessageQueueOnMessageFramesAdded;
            _messageQueues[messageQueue] = messageQueuePayload;
            StartAsyncProcessingIfNotStartedYet();
        }

        private void MessageQueueOnMessageFramesAdded(IReadOnlyCollection<MessageFrame> messageFrame)
        {
            _messageQueueHasMessage.Set();
        }

        public bool Unregister(MessageQueue messageQueue)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");

            MessageQueuePayload messageQueuePayload;
            var removed = _messageQueues.TryRemove(messageQueue, out messageQueuePayload);
            if (removed)
            {
                messageQueuePayload.Cancelled.Set(true);
                messageQueue.MessageFramesAdded -= MessageQueueOnMessageFramesAdded;
            }

            if (_messageQueues.IsEmpty)
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
                    _messageQueueHasMessage.WaitOne(10);

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
            foreach (var item in _messageQueues)
            {
                var messageQueue = item.Key;
                var messageQueuePayload = item.Value;

                if (messageQueuePayload.Busy.Set(true)) continue;
                var task = LoopMessageQueueAsync(messageQueue, messageQueuePayload, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task LoopMessageQueueAsync(MessageQueue messageQueue, MessageQueuePayload messageQueuePayload, CancellationToken cancellationToken)
        {
            try
            {
                await messageQueue.SendFromQueueAsync(messageQueuePayload.Writer, cancellationToken);
            }
            finally
            {
                messageQueuePayload.Busy.Set(false);
            }
        }

        private void StopProcessing()
        {
            var task = Interlocked.Exchange(ref _asyncSchedulerTask, null);
            if (task == null) return;

            _cts.Cancel(false);

            _started.Set(false);
        }
    }

    struct MessageQueuePayload
    {
        public IMessageFrameWriter Writer;
        public InterlockedBoolean Busy;
        public InterlockedBoolean Cancelled;

        public MessageQueuePayload(IMessageFrameWriter writer)
        {
            Writer = writer;
            Busy = new InterlockedBoolean();
            Cancelled = new InterlockedBoolean();
        }
    }
}
