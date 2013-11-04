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

namespace RedFoxMQ
{
    public class Responder : IBindSockets, IDisposable
    {
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();
        private static readonly MessageFrameCreator MessageFrameCreator = new MessageFrameCreator();
        
        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<MessageReceiveLoop, CancellationTokenSource> _clientSockets;
        private readonly MessageQueueProcessor _messageQueueProcessor = new MessageQueueProcessor();

        public Responder()
        {
            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _clientSockets = new ConcurrentDictionary<MessageReceiveLoop, CancellationTokenSource>();
        }

        public void Bind(RedFoxEndpoint endpoint)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, OnClientConnected);
            _servers[endpoint] = server;
        }

        private void OnClientConnected(ISocket socket)
        {
            var messageFrameSender = new MessageFrameSender(socket);
            var messageReceiveLoop = new MessageReceiveLoop(socket);
            var cancellationTokenSource = new CancellationTokenSource();
            _clientSockets.TryAdd(messageReceiveLoop, cancellationTokenSource);
        }

        public bool Unbind(RedFoxEndpoint endpoint)
        {
            ISocketAccepter removedServer;
            var serverRemoved = _servers.TryRemove(endpoint, out removedServer);
            if (serverRemoved) removedServer.Unbind();

            return serverRemoved;
        }

        private void UnbindAllEndpoints()
        {
            try
            {
                do
                {
                    var endpoints = _servers.Keys.ToList();

                    foreach (var endpoint in endpoints)
                    {
                        Unbind(endpoint);
                    }

                } while (true);
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

        ~Responder()
        {
            Dispose(false);
        }
        #endregion
    }
}
