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

using RedFoxMQ.Transports;
using System;
using System.Threading;

namespace RedFoxMQ
{
    /// <summary>
    /// Connects to a ServiceQueue to read messages
    /// </summary>
    public class ServiceQueueReader : IServiceQueueReader
    {
        private static readonly NodeGreetingMessageVerifier NodeGreetingMessageVerifier = new NodeGreetingMessageVerifier(NodeType.ServiceQueueReader, NodeType.ServiceQueue);
        private static readonly SocketFactory SocketFactory = new SocketFactory();

        private readonly IMessageSerialization _messageSerialization;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ISocket _socket;
        public ISocket Socket
        {
            get { return _socket; }
        }

        private MessageReceiveLoop _messageReceiveLoop;

        public bool IsDisconnected
        {
            get
            {
                var socket = _socket;
                return socket == null || socket.IsDisconnected;
            }
        }

        public event DisconnectedDelegate Disconnected = () => { };

        public event MessageReceivedDelegate MessageReceived = (socket, message) => { };
        public event SocketExceptionDelegate ResponseException = (socket, exception) => { };

        public ServiceQueueReader()
            : this(DefaultMessageSerialization.Instance)
        {

        }

        public ServiceQueueReader(IMessageSerialization messageSerialization)
        {
            if (messageSerialization == null) throw new ArgumentNullException("messageSerialization");
            _messageSerialization = messageSerialization;
        }

        public void Connect(RedFoxEndpoint endpoint)
        {
            Connect(endpoint, SocketConfiguration.Default);
        }

        public void Connect(RedFoxEndpoint endpoint, TimeSpan connectTimeout)
        {
            var socketConfiguration = (SocketConfiguration)SocketConfiguration.Default.Clone();
            socketConfiguration.ConnectTimeout = connectTimeout;

            Connect(endpoint, socketConfiguration);
        }

        public void Connect(RedFoxEndpoint endpoint, ISocketConfiguration socketConfiguration)
        {
            if (socketConfiguration == null) throw new ArgumentNullException("socketConfiguration");
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");
            _cts = new CancellationTokenSource();

            _socket = SocketFactory.CreateAndConnectAsync(endpoint, socketConfiguration);
            _socket.Disconnected += SocketDisconnected;

            NodeGreetingMessageVerifier.SendReceiveAndVerify(_socket, socketConfiguration.ConnectTimeout);

            if (!_cts.IsCancellationRequested)
            {
                _messageReceiveLoop = new MessageReceiveLoop(_messageSerialization, _socket);
                _messageReceiveLoop.MessageReceived += (s, m) => MessageReceived(s, m);
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

            var messageReceiveLoop = _messageReceiveLoop;
            if (messageReceiveLoop != null) messageReceiveLoop.Dispose();

            socket.Disconnect();
        }

        #region Dispose
        private bool _disposed;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (_disposed) return;

                Disconnect();

                _disposed = true;
                if (disposing) GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ServiceQueueReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
