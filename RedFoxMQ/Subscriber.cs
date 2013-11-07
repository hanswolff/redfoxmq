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
    public class Subscriber : ISubscriber
    {
        private static readonly SocketFactory SocketFactory = new SocketFactory();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ISocket _socket;
        private MessageReceiveLoop _messageReceiveLoop;

        public event Action<IMessage> MessageReceived = m => { };

        public async Task ConnectAsync(RedFoxEndpoint endpoint)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");

            await ConnectAsync(endpoint, 0);
        }

        public async Task ConnectAsync(RedFoxEndpoint endpoint, int timeoutInSeconds)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");
            _cts = new CancellationTokenSource();

            _socket = await SocketFactory.CreateAndConnect(endpoint, timeoutInSeconds);

            if (!_cts.IsCancellationRequested)
            {
                _messageReceiveLoop = new MessageReceiveLoop(_socket);
                _messageReceiveLoop.MessageReceived += m => MessageReceived(m);
                _messageReceiveLoop.Start();
            }
        }

        public void Disconnect()
        {
            Disconnect(false, TimeSpan.FromSeconds(5));
        }

        public void Disconnect(bool waitForExit, TimeSpan timeout)
        {
            var socket = Interlocked.Exchange(ref _socket, null);
            if (socket == null) return;

            _messageReceiveLoop.Stop(waitForExit);

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
