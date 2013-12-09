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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameSender : ISendMessageFrame, IDisconnect
    {
        private static readonly MessageFrameStreamWriter MessageFrameStreamWriter = new MessageFrameStreamWriter();

        private readonly ISocket _socket;
        public MessageFrameSender(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;
        }

        public async Task SendAsync(MessageFrame messageFrame, CancellationToken cancellationToken)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");

            await MessageFrameStreamWriter.WriteMessageFrameAsync(_socket.Stream, messageFrame, cancellationToken);
        }

        public async Task SendAsync(ICollection<MessageFrame> messageFrames, CancellationToken cancellationToken)
        {
            if (messageFrames == null) return;

            await MessageFrameStreamWriter.WriteMessageFramesAsync(_socket.Stream, messageFrames, cancellationToken);
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }
    }
}
