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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class ServiceQueue : IServiceQueue
    {
        private static readonly NodeGreetingMessageVerifier NodeGreetingMessageVerifier = new NodeGreetingMessageVerifier(NodeType.ServiceQueue, NodeType.ServiceQueueReader, NodeType.ServiceQueueWriter);
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();

        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<ISocket, IMessageFrameWriter> _readerClientSockets;
        private readonly ConcurrentDictionary<ISocket, MessageFrameReceiver> _writerClientSockets;
        private readonly ConcurrentQueue<MessageFrame> _queueMessageFrames = new ConcurrentQueue<MessageFrame>();
        private readonly CancellationTokenSource _disposedCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _disposedToken;

        public event ClientConnectedDelegate ClientConnected = (socket, socketConfig) => { };
        public event ClientDisconnectedDelegate ClientDisconnected = socket => { };
        public event MessageFrameReceivedDelegate MessageFrameReceived = m => { };

        public int MessageFramesCount { get { return _queueMessageFrames.Count; } }

        public ServiceQueue()
        {
            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _readerClientSockets = new ConcurrentDictionary<ISocket, IMessageFrameWriter>();
            _writerClientSockets = new ConcurrentDictionary<ISocket, MessageFrameReceiver>();

            _disposedToken = _disposedCancellationTokenSource.Token;
        }

        public void Bind(RedFoxEndpoint endpoint)
        {
            Bind(endpoint, SocketConfiguration.Default);
        }

        public void Bind(RedFoxEndpoint endpoint, ISocketConfiguration socketConfiguration)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, socketConfiguration, OnClientConnected, SocketDisconnected);
            _servers[endpoint] = server;
        }

        private static readonly MessageFrameWriterFactory MessageFrameWriterFactory = new MessageFrameWriterFactory();
        private void OnClientConnected(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            var remoteNodeType = NodeGreetingMessageVerifier.SendReceiveAndVerify(socket, socketConfiguration.ConnectTimeout).Result;
            switch (remoteNodeType)
            {
                case NodeType.ServiceQueueReader:
                    ReaderClientConnected(socket, socketConfiguration);
                    break;
                case NodeType.ServiceQueueWriter:
                    WriterClientConnected(socket, socketConfiguration);
                    break;
                default:
                    throw new RedFoxProtocolException(String.Format("Unsupported node type connected to ServiceQueue: {0}", remoteNodeType));
            }

            if (socket.IsDisconnected)
            {
                // this is to fix the race condition if socket was disconnected meanwhile
                SocketDisconnected(socket);
            }
        }

        private void ReaderClientConnected(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            var messageFrameWriter = MessageFrameWriterFactory.CreateWriterFromSocket(socket);

            if (_readerClientSockets.TryAdd(socket, messageFrameWriter))
            {
                ClientConnected(socket, socketConfiguration);
            }
        }

        private void WriterClientConnected(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            var messageFrameReceiver = new MessageFrameReceiver(socket);
            messageFrameReceiver.Disconnected += () => WriterSocketDisconnected(socket);
            var task = ReceiveAsync(messageFrameReceiver, _disposedToken);

            if (_writerClientSockets.TryAdd(socket, messageFrameReceiver))
            {
                ClientConnected(socket, socketConfiguration);
            }
        }

        private async Task ReceiveAsync(MessageFrameReceiver messageFrameReceiver, CancellationToken cancellationToken)
        {
            var task = messageFrameReceiver.ReceiveAsync(cancellationToken);
            var messageFrame = await task;
            _queueMessageFrames.Enqueue(messageFrame);
            MessageFrameReceived(messageFrame);

            var newTask = ReceiveAsync(messageFrameReceiver, cancellationToken);
        }

        private void MessageReceiveLoopOnException(ISocket socket, Exception exception)
        {
            socket.Disconnect();
        }

        private void SocketDisconnected(ISocket socket)
        {
            var disconnected = ReaderSocketDisconnected(socket) && WriterSocketDisconnected(socket);
        }

        private bool ReaderSocketDisconnected(ISocket socket)
        {
            IMessageFrameWriter messageFrameWriter;
            if (_readerClientSockets.TryRemove(socket, out messageFrameWriter))
            {
                ClientDisconnected(socket);
                return true;
            }
            return false;
        }

        private bool WriterSocketDisconnected(ISocket socket)
        {
            MessageFrameReceiver messageFrameReceiver;
            if (_writerClientSockets.TryRemove(socket, out messageFrameReceiver))
            {
                ClientDisconnected(socket);
                return true;
            }
            return false;
        }

        public bool Unbind(RedFoxEndpoint endpoint)
        {
            ISocketAccepter removedServer;
            var serverRemoved = _servers.TryRemove(endpoint, out removedServer);
            if (serverRemoved)
            {
                removedServer.Unbind();
                DisconnectSocketsForEndpoint(endpoint);
            }

            return serverRemoved;
        }

        private void DisconnectSocketsForEndpoint(RedFoxEndpoint endpoint)
        {
            var socketsMatchingEndpoint = _readerClientSockets.Keys.Where(socket => socket.Endpoint.Equals(endpoint));
            foreach (var socket in socketsMatchingEndpoint)
            {
                socket.Disconnect();
            }
        }

        private void UnbindAllEndpoints()
        {
            try
            {
                var endpoints = _servers.Keys.ToList();

                foreach (var endpoint in endpoints)
                {
                    Unbind(endpoint);
                }
            }
            catch (InvalidOperationException) { }
        }

        #region Dispose
        private bool _disposed;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (_disposed) return;

                _disposedCancellationTokenSource.Cancel();

                UnbindAllEndpoints();

                _disposed = true;
                if (disposing) GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ServiceQueue()
        {
            Dispose(false);
        }
        #endregion
    }
}
