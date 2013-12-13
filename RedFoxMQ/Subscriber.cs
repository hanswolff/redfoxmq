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

namespace RedFoxMQ
{
    public class Subscriber : ISubscriber
    {
        private static readonly SocketFactory SocketFactory = new SocketFactory();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ISocket _socket;
        private MessageReceiveLoop _messageReceiveLoop;

        public bool IsDisconnected { get { return _socket.IsDisconnected; } }

        public event Action Disconnected = () => { };

        public event Action<IMessage> MessageReceived = m => { };
        public event Action<ISocket, Exception> ResponseException = (socket, exception) => { };

        public void Connect(RedFoxEndpoint endpoint)
        {
            Connect(endpoint, TimeSpan.FromMilliseconds(-1));
        }

        public void Connect(RedFoxEndpoint endpoint, TimeSpan timeout)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");
            _cts = new CancellationTokenSource();

            _socket = SocketFactory.CreateAndConnectAsync(endpoint, timeout);
            _socket.Disconnected += SocketDisconnected;

            if (!_cts.IsCancellationRequested)
            {
                _messageReceiveLoop = new MessageReceiveLoop(_socket);
                _messageReceiveLoop.MessageReceived += m => MessageReceived(m);
                _messageReceiveLoop.OnException += MessageReceiveLoopOnException;
                _messageReceiveLoop.Start();
            }
        }

        private void MessageReceiveLoopOnException(ISocket socket, Exception exception)
        {
            try { ResponseException(socket, exception); }
            catch { }

            socket.Disconnect();
        }

        private void SocketDisconnected()
        {
            Disconnected();
        }

        public void Disconnect()
        {
            Disconnect(false, TimeSpan.FromSeconds(5));
        }

        public void Disconnect(bool waitForExit, TimeSpan timeout)
        {
            var socket = Interlocked.Exchange(ref _socket, null);
            if (socket == null) return;

            _messageReceiveLoop.Dispose();

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
