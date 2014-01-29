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
        private readonly ConcurrentQueue<MessageFrameWriterPayload> _messageWritersQueueForRoundRobin = new ConcurrentQueue<MessageFrameWriterPayload>();
        private readonly Action<CancellationToken> _rotationFunc;

        private Task _asyncSchedulerTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public MessageQueueDistributor(MessageQueue messageQueue, ServiceQueueRotationAlgorithm rotationAlgorithm)
        {
            if (messageQueue == null) throw new ArgumentNullException("messageQueue");
            _messageQueue = messageQueue;
            _rotationFunc = GetRotationFunc(rotationAlgorithm);
        }

        private Action<CancellationToken> GetRotationFunc(ServiceQueueRotationAlgorithm rotationAlgorithm)
        {
            switch (rotationAlgorithm)
            {
                case ServiceQueueRotationAlgorithm.FirstIdle:
                    return LoopFirstIdleAsync;
                case ServiceQueueRotationAlgorithm.LoadBalance:
                    return LoopLoadBalanceAsync;
                default:
                    throw new NotSupportedException(String.Format("Unsupported ServiceQueueRotationAlgorithm: {0}",
                        rotationAlgorithm));
            }
        }

        public void Register(IMessageFrameWriter messageFrameWriter)
        {
            if (messageFrameWriter == null) throw new ArgumentNullException("messageFrameWriter");

            var messageQueuePayload = new MessageFrameWriterPayload(messageFrameWriter);
            if (_messageFrameWriters.TryAdd(messageFrameWriter, messageQueuePayload))
            {
                _messageWritersQueueForRoundRobin.Enqueue(messageQueuePayload);
            }

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
                    _rotationFunc(cancellationToken);
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

        private void LoopFirstIdleAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _messageFrameWriters)
            {
                var messageFrameWriterPayload = item.Value;

                if (messageFrameWriterPayload.Busy.Set(true)) continue;
                var task = LoopMessageQueueAsync(messageFrameWriterPayload, cancellationToken);
                return;
            }
        }

        private void LoopLoadBalanceAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                MessageFrameWriterPayload messageFrameWriterPayload;
                if (!_messageWritersQueueForRoundRobin.TryDequeue(out messageFrameWriterPayload)) continue;

                if (messageFrameWriterPayload.Cancelled.Value) continue;
                if (messageFrameWriterPayload.Busy.Set(true)) continue;

                var task = LoopMessageQueueAsync(
                    messageFrameWriterPayload, 
                    cancellationToken, 
                    () => _messageWritersQueueForRoundRobin.Enqueue(messageFrameWriterPayload));

                return;
            }
        }

        private async Task LoopMessageQueueAsync(MessageFrameWriterPayload messageFrameWriterPayload, CancellationToken cancellationToken, Action finalAction = null)
        {
            try
            {
                await _messageQueue.SendFromQueueAsync(messageFrameWriterPayload.Writer, cancellationToken);
            }
            finally
            {
                messageFrameWriterPayload.Busy.Set(false);
                if (finalAction != null) finalAction();
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
