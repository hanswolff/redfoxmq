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
    class MessageFrameReceiver : IReceiveMessageFrame, IDisconnect
    {
        private static readonly MessageFrameReaderFactory MessageFrameReaderFactory = new MessageFrameReaderFactory();
        private readonly IMessageFrameReader _messageFrameReader;

        private readonly ISocket _socket;
        public MessageFrameReceiver(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _messageFrameReader = MessageFrameReaderFactory.CreateReaderFromSocket(socket);

            _socket = socket;
            _socket.Disconnected += SocketDisconnected;
        }

        private void SocketDisconnected()
        {
            Disconnected();
        }

        public async Task<MessageFrame> ReceiveAsync(CancellationToken cancellationToken)
        {
            return await _messageFrameReader.ReadMessageFrameAsync(cancellationToken);
        }

        public MessageFrame Receive()
        {
            return _messageFrameReader.ReadMessageFrame();
        }

        public bool IsDisconnected { get { return _socket.IsDisconnected; } }

        public event DisconnectedDelegate Disconnected = () => { };

        public void Disconnect()
        {
            _socket.Disconnect();
        }
    }
}
