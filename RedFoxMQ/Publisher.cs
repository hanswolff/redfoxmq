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
using System.Collections.Generic;
using System.Linq;

namespace RedFoxMQ
{
    public class Publisher : IPublisher
    {
        private static readonly NodeGreetingMessageVerifier NodeGreetingMessageVerifier = new NodeGreetingMessageVerifier(NodeType.Publisher, NodeType.Subscriber);
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();

        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<ISocket, MessageQueueReceiveLoop> _broadcastSockets;
        private readonly MessageFrameCreator _messageFrameCreator;
        private readonly MessageQueueBatchBroadcaster _messageQueueBroadcaster = new MessageQueueBatchBroadcaster();
        private readonly IMessageSerialization _messageSerialization;

        public event ClientConnectedDelegate ClientConnected = (socket, socketConfig) => { };
        public event ClientDisconnectedDelegate ClientDisconnected = socket => { };
        public event MessageReceivedDelegate MessageReceived = (socket, message) => { };

        public Publisher()
            : this(DefaultMessageSerialization.Instance)
        {
        }

        public Publisher(IMessageSerialization messageSerialization)
        {
            if (messageSerialization == null) throw new ArgumentNullException("messageSerialization");

            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _broadcastSockets = new ConcurrentDictionary<ISocket, MessageQueueReceiveLoop>();
            _messageSerialization = messageSerialization;
            _messageFrameCreator = new MessageFrameCreator(messageSerialization);
        }

        public void Bind(RedFoxEndpoint endpoint)
        {
            Bind(endpoint, SocketConfiguration.Default);
        }

        public void Bind(RedFoxEndpoint endpoint, ISocketConfiguration socketConfiguration)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, NodeType.Publisher, socketConfiguration, OnClientConnected, SocketDisconnected);
            _servers[endpoint] = server;
        }

        private static readonly MessageFrameWriterFactory MessageFrameWriterFactory = new MessageFrameWriterFactory();
        private void OnClientConnected(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            NodeGreetingMessageVerifier.SendReceiveAndVerify(socket, socketConfiguration.ConnectTimeout);

            var messageFrameWriter = MessageFrameWriterFactory.CreateWriterFromSocket(socket);
            var messageQueue = new MessageQueueBatch(socketConfiguration.SendBufferSize);
            var messageReceiveLoop = new MessageReceiveLoop(_messageSerialization, socket, OnMessageReceived, MessageReceiveLoopOnException);

            if (_broadcastSockets.TryAdd(socket, new MessageQueueReceiveLoop(messageQueue, messageReceiveLoop)))
            {
                _messageQueueBroadcaster.Register(messageQueue, messageFrameWriter);
                ClientConnected(socket, socketConfiguration);
                messageReceiveLoop.Start();
            }

            if (socket.IsDisconnected)
            {
                // this is to fix the race condition if socket was disconnected meanwhile
                SocketDisconnected(socket);
            }
        }

        private void OnMessageReceived(ISocket socket, IMessage message)
        {
            MessageReceived(socket, message);
        }

        private void MessageReceiveLoopOnException(ISocket socket, Exception exception)
        {
            socket.Disconnect();
        }

        private void SocketDisconnected(ISocket socket)
        {
            MessageQueueReceiveLoop messageQueueReceiveLoop;
            if (_broadcastSockets.TryRemove(socket, out messageQueueReceiveLoop))
            {
                _messageQueueBroadcaster.Unregister(messageQueueReceiveLoop.MessageQueue);
                messageQueueReceiveLoop.MessageReceiveLoop.Dispose();
                ClientDisconnected(socket);
            }
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
            var socketsMatchingEndpoint = _broadcastSockets.Keys.Where(socket => socket.Endpoint.Equals(endpoint));
            foreach (var socket in socketsMatchingEndpoint)
            {
                socket.Disconnect();
            }
        }

        public void Broadcast(IMessage message)
        {
            var messageFrame = _messageFrameCreator.CreateFromMessage(message);

            foreach (var messageQueue in _broadcastSockets.Values)
            {
                messageQueue.MessageQueue.Add(messageFrame);
            }
        }

        public void Broadcast(IReadOnlyList<IMessage> messages)
        {
            if (messages == null) return;
            var messageFrames = messages.Select(message => _messageFrameCreator.CreateFromMessage(message)).ToList();

            foreach (var messageQueue in _broadcastSockets.Values)
            {
                messageQueue.MessageQueue.AddRange(messageFrames);
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

                UnbindAllEndpoints();

                _disposed = true;
                if (disposing) GC.SuppressFinalize(this);
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

        class MessageQueueReceiveLoop
        {
            public readonly MessageQueueBatch MessageQueue;
            public readonly MessageReceiveLoop MessageReceiveLoop;

            public MessageQueueReceiveLoop(MessageQueueBatch messageQueue, MessageReceiveLoop messageReceiveLoop)
            {
                MessageQueue = messageQueue;
                MessageReceiveLoop = messageReceiveLoop;
            }
        }
    }
}
