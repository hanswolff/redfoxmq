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
        private Task _executeTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Register(MessageQueue messageQueue, MessageFrameSender sender)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            if (sender == null) throw new ArgumentNullException("sender");

            _messageQueues[messageQueue] = new MessageQueuePayload(sender);
            messageQueue.MessageFramesAdded += MessageQueueOnMessageFramesAdded;

            StartProcessingIfNotStartedYet();
        }

        private void MessageQueueOnMessageFramesAdded(IReadOnlyCollection<MessageFrame> messageFrame)
        {
            _messageQueueHasMessage.Set();
        }

        public bool Unregister(MessageQueue messageQueue)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");

            MessageQueuePayload oldValue;
            var removed = _messageQueues.TryRemove(messageQueue, out oldValue);
            if (removed)
            {
                messageQueue.MessageFramesAdded -= MessageQueueOnMessageFramesAdded;
            }

            if (_messageQueues.IsEmpty)
            {
                StopProcessing();
            }

            return removed;
        }

        private readonly InterlockedBoolean _started = new InterlockedBoolean();
        private void StartProcessingIfNotStartedYet()
        {
            if (_started.Set(true)) return;

            try
            {
                _cts = new CancellationTokenSource();
                _executeTask = Task.Factory.StartNew(() => Execute(_cts.Token), TaskCreationOptions.LongRunning);
            }
            catch (Exception)
            {
                _started.Set(false);
                throw;
            }
        }

        private void Execute(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _messageQueueHasMessage.WaitOne(10);

                    Loop(cancellationToken);
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

        private void Loop(CancellationToken cancellationToken)
        {
            foreach (var item in _messageQueues)
            {
                var messageQueue = item.Key;
                var messageQueuePayload = item.Value;

                if (messageQueuePayload.Busy.Set(true)) continue;
                var task = LoopMessageQueue(messageQueue, messageQueuePayload.Sender, messageQueuePayload.Busy, cancellationToken);
            }
        }

        private static async Task LoopMessageQueue(MessageQueue messageQueue, MessageFrameSender sender, InterlockedBoolean busy, CancellationToken cancellationToken)
        {
            try
            {
                var hasMore = true;
                while (hasMore && !cancellationToken.IsCancellationRequested)
                {
                    hasMore = await messageQueue.SendFromQueueAsync(sender, cancellationToken);
                }
            }
            finally
            {
                busy.Set(false);
            }
        }

        private void StopProcessing()
        {
            var task = Interlocked.Exchange(ref _executeTask, null);
            if (task == null) return;

            _cts.Cancel(false);

            _started.Set(false);
        }
    }

    struct MessageQueuePayload
    {
        public MessageFrameSender Sender;
        public InterlockedBoolean Busy;

        public MessageQueuePayload(MessageFrameSender sender)
        {
            Sender = sender;
            Busy = new InterlockedBoolean();
        }
    }
}
