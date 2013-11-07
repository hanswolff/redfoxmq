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
        private static readonly SocketFactory SocketFactory = new SocketFactory();
        private static readonly MessageFrameCreator MessageFrameCreator = new MessageFrameCreator();

        private MessageFrameSender _messageFrameSender;
        private MessageFrameReceiver _messageFrameReceiver;

        private ISocket _socket;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public async Task ConnectAsync(RedFoxEndpoint endpoint)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");

            _cts = new CancellationTokenSource();

            await ConnectAsync(endpoint, 0);
        }

        public async Task ConnectAsync(RedFoxEndpoint endpoint, int timeoutInSeconds)
        {
            if (_socket != null) throw new InvalidOperationException("Subscriber already connected");
            _cts = new CancellationTokenSource();

            _socket = await SocketFactory.CreateAndConnect(endpoint, timeoutInSeconds);

            if (!_cts.IsCancellationRequested)
            {
                _messageFrameSender = new MessageFrameSender(_socket);
                _messageFrameReceiver = new MessageFrameReceiver(_socket);
            }
        }

        public async Task<IMessage> Request(IMessage message)
        {
            return await RequestWithCancellationToken(message, _cts.Token);
        }

        public async Task<IMessage> Request(IMessage message, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
            {
                return await RequestWithCancellationToken(message, cts.Token);
            }
        }

        private async Task<IMessage> RequestWithCancellationToken(IMessage message, CancellationToken cancellationToken)
        {
            var sendMessageFrame = MessageFrameCreator.CreateFromMessage(message);
            await _messageFrameSender.SendAsync(sendMessageFrame, cancellationToken);

            var responseMessageFrame = await _messageFrameReceiver.ReceiveAsync(cancellationToken);
            var response = MessageSerialization.Instance.Deserialize(responseMessageFrame.MessageTypeId,
                responseMessageFrame.RawMessage);
            return response;
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
