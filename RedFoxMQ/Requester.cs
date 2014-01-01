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
    public class Requester : IRequester
    {
        private static readonly MessageFrameCreator MessageFrameCreator = new MessageFrameCreator();
        private static readonly SocketFactory SocketFactory = new SocketFactory();

        private MessageFrameSender _messageFrameSender;
        private MessageFrameReceiver _messageFrameReceiver;

        private ISocket _socket;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public bool IsDisconnected
        {
            get
            {
                var socket = _socket;
                return socket == null || socket.IsDisconnected;
            }
        }

        public event DisconnectedDelegate Disconnected = () => { };

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
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");
            _cts = new CancellationTokenSource();

            _socket = SocketFactory.CreateAndConnectAsync(endpoint, socketConfiguration);
            _socket.Disconnected += SocketDisconnected;

            if (!_cts.IsCancellationRequested)
            {
                _messageFrameSender = new MessageFrameSender(_socket);
                _messageFrameReceiver = new MessageFrameReceiver(_socket);
            }
        }

        private void SocketDisconnected()
        {
            Disconnected();
        }

        public IMessage Request(IMessage requestMessage)
        {
            var sendMessageFrame = MessageFrameCreator.CreateFromMessage(requestMessage);

            _semaphoreRequest.Wait(_cts.Token);
            try
            {
                _messageFrameSender.Send(sendMessageFrame);

                var messageFrame = _messageFrameReceiver.Receive();
                var responseMessage = MessageSerialization.Instance.Deserialize(messageFrame.MessageTypeId, messageFrame.RawMessage);
                return responseMessage;
            }
            finally
            {
                _semaphoreRequest.Release();
            }
        }

        public async Task<IMessage> RequestAsync(IMessage message)
        {
            return await RequestWithCancellationToken(message, _cts.Token);
        }

        public async Task<IMessage> RequestAsync(IMessage message, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
            {
                return await RequestWithCancellationToken(message, cts.Token).ConfigureAwait(false);
            }
        }

        private readonly SemaphoreSlim _semaphoreRequest = new SemaphoreSlim(1);
        private async Task<IMessage> RequestWithCancellationToken(IMessage message, CancellationToken cancellationToken)
        {
            await _semaphoreRequest.WaitAsync(cancellationToken);
            try
            {
                var sendMessageFrame = MessageFrameCreator.CreateFromMessage(message);
                await _messageFrameSender.SendAsync(sendMessageFrame, cancellationToken);

                var messageFrame = await _messageFrameReceiver.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                var responseMessage = MessageSerialization.Instance.Deserialize(messageFrame.MessageTypeId, messageFrame.RawMessage);
                return responseMessage;
            }
            finally
            {
                _semaphoreRequest.Release();
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

        ~Requester()
        {
            Dispose(false);
        }
        #endregion

    }
}
