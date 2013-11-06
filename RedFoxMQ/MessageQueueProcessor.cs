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
        private readonly ConcurrentDictionary<MessageQueue, ManualResetEventSlim> _messageQueues = new ConcurrentDictionary<MessageQueue, ManualResetEventSlim>();
        private readonly AutoResetEvent _messageQueueHasMessage = new AutoResetEvent(false);
        private Task _executeTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Register(MessageQueue messageQueue)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            _messageQueues[messageQueue] = new ManualResetEventSlim(false);
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

            ManualResetEventSlim oldValue;
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

        private int _started;
        private void StartProcessingIfNotStartedYet()
        {
            var alreadyStarted = Interlocked.Exchange(ref _started, 1);
            if (alreadyStarted == 1) return;

            _cts = new CancellationTokenSource();
            _executeTask = Task.Factory.StartNew(() => Execute(_cts.Token), TaskCreationOptions.LongRunning);
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
        }

        private void Loop(CancellationToken cancellationToken)
        {
            foreach (var item in _messageQueues)
            {
                var isBusy = item.Value;
                if (isBusy.IsSet) continue;

                isBusy.Set();
                try
                {
                    var messageQueue = item.Key;
                    var task = LoopMessageQueue(messageQueue, cancellationToken);
                }
                finally
                {
                    isBusy.Reset();
                }
            }
        }

        private static async Task LoopMessageQueue(MessageQueue messageQueue, CancellationToken cancellationToken)
        {
            var hasMore = true;
            while (hasMore && !cancellationToken.IsCancellationRequested)
            {
                hasMore = await messageQueue.SendFromQueue(cancellationToken);
            }
        }

        private void StopProcessing()
        {
            var task = Interlocked.Exchange(ref _executeTask, null);
            if (task == null) return;

            _cts.Cancel(false);

            Interlocked.Exchange(ref _started, 0);
        }
    }
}
