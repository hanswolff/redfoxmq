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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    public class Responder : IResponder
    {
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();
        private static readonly MessageFrameCreator MessageFrameCreator = new MessageFrameCreator();

        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<ISocket, SenderReceiver> _clientSockets;
        private readonly IResponderWorkerFactory _responderWorkerFactory;
        private readonly ResponderWorkerScheduler _scheduler;

        public Responder(IResponderWorkerFactory responderWorkerFactory, int minThreads = 1, int maxThreads = 1)
        {
            if (responderWorkerFactory == null) throw new ArgumentNullException("responderWorkerFactory");
            _responderWorkerFactory = responderWorkerFactory;

            _disposeCancellationTokenSource = new CancellationTokenSource();
            _disposeCancellationToken = _disposeCancellationTokenSource.Token;

            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _clientSockets = new ConcurrentDictionary<ISocket, SenderReceiver>();
            _scheduler = new ResponderWorkerScheduler(minThreads, maxThreads);
            _scheduler.WorkerCompleted += SchedulerWorkerCompleted;
        }

        public event ClientConnectedDelegate ClientConnected = (socket, socketConfig) => { };
        public event ClientDisconnectedDelegate ClientDisconnected = s => { };

        public void Bind(RedFoxEndpoint endpoint)
        {
            Bind(endpoint, SocketConfiguration.Default);
        }

        public void Bind(RedFoxEndpoint endpoint, ISocketConfiguration socketConfiguration)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, socketConfiguration, OnClientConnected);
            _servers[endpoint] = server;
        }

        private static readonly MessageFrameWriterFactory MessageFrameWriterFactory = new MessageFrameWriterFactory();
        private void OnClientConnected(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            var messageFrameWriter = MessageFrameWriterFactory.CreateWriterFromSocket(socket);
            var messageFrameReceiver = new MessageFrameReceiver(socket);
            var senderReceiver = new SenderReceiver(messageFrameWriter, messageFrameReceiver);

            socket.Disconnected += () => SocketDisconnected(socket);

            if (_clientSockets.TryAdd(socket, senderReceiver))
            {
                var task = ReceiveRequestMessage(senderReceiver);
                ClientConnected(socket, socketConfiguration);
            }

            if (socket.IsDisconnected)
            {
                // this is to fix the race condition if socket was disconnected meanwhile
                SocketDisconnected(socket);
            }
        }

        private void SocketDisconnected(ISocket socket)
        {
            SenderReceiver senderReceiver;
            if (_clientSockets.TryRemove(socket, out senderReceiver))
            {
                ClientDisconnected(socket);
            }
        }

        private async Task ReceiveRequestMessage(SenderReceiver senderReceiver)
        {
            if (senderReceiver.Receiver == null) throw new ArgumentException("senderReceiver.Receiver must not be null");
            if (senderReceiver.Sender == null) throw new ArgumentException("senderReceiver.Writer must not be null");

            var messageFrame = await senderReceiver.Receiver.ReceiveAsync(_disposeCancellationToken).ConfigureAwait(false);
            var requestMessage = MessageSerialization.Instance.Deserialize(messageFrame.MessageTypeId,
                messageFrame.RawMessage);

            var worker = _responderWorkerFactory.GetWorkerFor(requestMessage);
            _scheduler.AddWorker(worker, requestMessage, senderReceiver);
        }

        private void SchedulerWorkerCompleted(IResponderWorker worker, object state, IMessage responseMessage)
        {
            var senderReceiver = (SenderReceiver)state;
            var responseFrame = MessageFrameCreator.CreateFromMessage(responseMessage);
            senderReceiver.Sender.WriteMessageFrame(responseFrame);

            var task = ReceiveRequestMessage(senderReceiver).ConfigureAwait(false);
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
            var socketsMatchingEndpoint = _clientSockets.Keys.Where(socket => socket.Endpoint.Equals(endpoint));
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
        private readonly CancellationTokenSource _disposeCancellationTokenSource;
        private readonly CancellationToken _disposeCancellationToken;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (_disposed) return;

                _disposeCancellationTokenSource.Cancel();

                UnbindAllEndpoints();

                _disposed = true;
                if (disposing) GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Responder()
        {
            Dispose(false);
        }
        #endregion
    }

    struct SenderReceiver
    {
        public MessageFrameReceiver Receiver;
        public IMessageFrameWriter Sender;

        public SenderReceiver(IMessageFrameWriter sender, MessageFrameReceiver receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }
    }
}
