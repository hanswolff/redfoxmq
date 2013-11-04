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
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    public class Subscriber : IConnectToEndpoint, IDisconnect, IDisposable
    {
        private static readonly SocketFactory SocketFactory = new SocketFactory();
        private MessageFrameReceiver _messageFrameReceiver;

        private ISocket _socket;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public event Action<IMessage> MessageReceived = m => { };

        public void Connect(RedFoxEndpoint endpoint)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");

            _socket = SocketFactory.CreateAndConnect(endpoint);
            _messageFrameReceiver = new MessageFrameReceiver(_socket);

            _cts = new CancellationTokenSource();
            
            _started.Reset();
            Task.Factory.StartNew(StartReceiveLoop, TaskCreationOptions.LongRunning);
            _started.Wait();
        }

        private void StartReceiveLoop()
        {
            var task = ReceiveLoopAsync(_cts.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            _stopped.Reset();
            _started.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var messageFrame = await _messageFrameReceiver.ReceiveAsync(cancellationToken);

                    IMessage message = null;
                    try
                    {
                        message = MessageSerialization.Instance.Deserialize(messageFrame.MessageTypeId, messageFrame.RawMessage);
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
            finally
            {
                _stopped.Set();
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

        public void Disconnect()
        {
            Disconnect(false);
        }

        public void Disconnect(bool waitForExit)
        {
            var socket = Interlocked.Exchange(ref _socket, null);
            if (socket == null) return;

            _cts.Cancel(false);

            if (waitForExit) _stopped.Wait();

            socket.Disconnect();
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
                    Disconnect();

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Subscriber()
        {
            Dispose(false);
        }
        #endregion

    }
}
