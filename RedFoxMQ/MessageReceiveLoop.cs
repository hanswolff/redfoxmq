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

using RedFoxMQ.Transports;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageReceiveLoop : IDisposable
    {
        private readonly MessageFrameReceiver _messageFrameReceiver;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public event Action<IMessage> MessageReceived = m => { };
        public event Action<ISocket, Exception> SocketError = (s, e) => { };

        private readonly ISocket _socket;

        public MessageReceiveLoop(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;
            _messageFrameReceiver = new MessageFrameReceiver(socket);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();

            _started.Reset();
            StartReceiveLoop();
            _started.Wait();
        }

        public void Stop(bool waitForExit = true)
        {
            _cts.Cancel(false);

            if (waitForExit) _stopped.Wait();
        }

        private void StartReceiveLoop()
        {
            Task.Factory.StartNew(() => ReceiveLoop(_cts.Token), TaskCreationOptions.LongRunning);
        }

        private void ReceiveLoop(CancellationToken cancellationToken)
        {
            _stopped.Reset();
            _started.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var messageFrame = _messageFrameReceiver.Receive();

                    IMessage message = null;
                    try
                    {
                        message = MessageSerialization.Instance.Deserialize(messageFrame.MessageTypeId,
                            messageFrame.RawMessage);
                    }
                    catch (RedFoxBaseException)
                    {
                        // TODO: pass on exception somewhere
                    }

                    FireMessageReceivedEvent(message);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException ex)
            {
                SocketError(_socket, ex);
            }
            finally
            {
                _stopped.Set();
                _started.Reset();
            }
        }

        private void FireMessageReceivedEvent(IMessage message)
        {
            if (message == null) return;

            try
            {
                MessageReceived(message);
            }
            catch
            {
            }
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
                    _messageFrameReceiver.Disconnect();
                    Stop(false);

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(false);
        }

        ~MessageReceiveLoop()
        {
            Dispose(false);
        }
        #endregion

    }
}
