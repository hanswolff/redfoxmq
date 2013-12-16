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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RedFoxMQ
{
    public class Publisher : IPublisher
    {
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();
        private static readonly MessageFrameCreator MessageFrameCreator = new MessageFrameCreator();

        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<ISocket, MessageQueue> _broadcastSockets;
        private readonly MessageQueueProcessor _messageQueueProcessor = new MessageQueueProcessor();

        public event Action<ISocket> ClientConnected = s => { };
        public event Action<ISocket> ClientDisconnected = s => { };

        public Publisher()
        {
            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _broadcastSockets = new ConcurrentDictionary<ISocket, MessageQueue>();
        }

        public void Bind(RedFoxEndpoint endpoint)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, SocketMode.WriteOnly, OnClientConnected, SocketDisconnected);
            _servers[endpoint] = server;
        }

        private void OnClientConnected(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            var messageFrameSender = new MessageFrameSender(socket);
            var messageQueue = new MessageQueue();

            if (_broadcastSockets.TryAdd(socket, messageQueue))
            {
                _messageQueueProcessor.Register(messageQueue, messageFrameSender);
                ClientConnected(socket);
            }

            if (socket.IsDisconnected)
            {
                // this is to fix the race condition if socket was disconnected meanwhile
                SocketDisconnected(socket);
            }
        }

        private void SocketDisconnected(ISocket socket)
        {
            MessageQueue messageQueue;
            if (_broadcastSockets.TryRemove(socket, out messageQueue))
            {
                _messageQueueProcessor.Unregister(messageQueue);
                ClientDisconnected(socket);
            }
        }

        public bool Unbind(RedFoxEndpoint endpoint)
        {
            ISocketAccepter removedServer;
            var serverRemoved = _servers.TryRemove(endpoint, out removedServer);
            if (serverRemoved) removedServer.Unbind();

            return serverRemoved;
        }

        public void Broadcast(IMessage message)
        {
            var messageFrame = MessageFrameCreator.CreateFromMessage(message);

            foreach (var messageQueue in _broadcastSockets.Values)
            {
                messageQueue.Add(messageFrame);
            }
        }

        public void Broadcast(IReadOnlyList<IMessage> messages)
        {
            if (messages == null) return;
            var messageFrames = messages.Select(message => MessageFrameCreator.CreateFromMessage(message)).ToList();

            foreach (var messageQueue in _broadcastSockets.Values)
            {
                messageQueue.AddRange(messageFrames);
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
                if (!_disposed)
                {
                    UnbindAllEndpoints();

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Publisher()
        {
            Dispose(false);
        }
        #endregion
    }
}
