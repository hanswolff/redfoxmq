using RedFoxMQ.Transports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RedFoxMQ
{
    public class Publisher : IBindSockets, IDisposable
    {
        private readonly ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter> _servers;
        private readonly ConcurrentDictionary<MessageQueue, CancellationTokenSource> _broadcastSockets;
        private static readonly SocketAccepterFactory SocketAccepterFactory = new SocketAccepterFactory();
        private readonly MessageQueueProcessor _messageQueueProcessor = new MessageQueueProcessor();

        public Publisher()
        {
            _servers = new ConcurrentDictionary<RedFoxEndpoint, ISocketAccepter>();
            _broadcastSockets = new ConcurrentDictionary<MessageQueue, CancellationTokenSource>();
        }

        public void Bind(RedFoxEndpoint endpoint)
        {
            var server = SocketAccepterFactory.CreateAndBind(endpoint, OnClientConnected);
            _servers[endpoint] = server;
        }

        private void OnClientConnected(ISocket socket)
        {
            var messageFrameSender = new MessageFrameSender(socket);
            var messageQueue = new MessageQueue(_messageQueueProcessor, messageFrameSender);
            var cancellationTokenSource = new CancellationTokenSource();
            _broadcastSockets.TryAdd(messageQueue, cancellationTokenSource);
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
            var alreadyBroadcastedTo = new HashSet<MessageQueue>();

            var messageFrame = CreateMessageFrame(message);

            foreach (var messageQueue in _broadcastSockets.Keys)
            {
                if (alreadyBroadcastedTo.Contains(messageQueue)) continue;
                alreadyBroadcastedTo.Add(messageQueue);

                messageQueue.Add(messageFrame);
            }
        }

        private static MessageFrame CreateMessageFrame(IMessage message)
        {
            var messageEvelope = new MessageFrame
            {
                MessageTypeId = message.MessageTypeId,
                TimestampSent = DateTime.UtcNow,
                RawMessage = MessageSerialization.Instance.Serialize(message)
            };
            return messageEvelope;
        }

        private void UnbindAllEndpoints()
        {
            try
            {
                do
                {
                    var endpoint = _servers.Keys.First();
                    if (!Unbind(endpoint)) continue;

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

        ~Publisher()
        {
            Dispose(false);
        }
        #endregion
    }
}
