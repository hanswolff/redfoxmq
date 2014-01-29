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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    [DebuggerDisplay("MessageFrameQueueWriter: {_queueSocket}")]
    class MessageFrameQueueWriter : IMessageFrameWriter
    {
        private readonly IQueueSocket _queueSocket;
        public MessageFrameQueueWriter(IQueueSocket queueSocket)
        {
            if (queueSocket == null) throw new ArgumentNullException("queueSocket");
            _queueSocket = queueSocket;
        }

        public void WriteMessageFrame(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (messageFrame.RawMessage == null) throw new ArgumentException("messageFrame.RawMessage cannot be null");

            _queueSocket.Write(messageFrame);
        }

        public async Task WriteMessageFrameAsync(MessageFrame messageFrame, CancellationToken cancellationToken)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (messageFrame.RawMessage == null) throw new ArgumentException("messageFrame.RawMessage cannot be null");

            _queueSocket.Write(messageFrame);
        }

        public void WriteMessageFrames(ICollection<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;

            foreach (var messageFrame in messageFrames)
            {
                _queueSocket.Write(messageFrame);
            }
        }

        public async Task WriteMessageFramesAsync(ICollection<MessageFrame> messageFrames, CancellationToken cancellationToken)
        {
            if (messageFrames == null) return;

            foreach (var messageFrame in messageFrames)
            {
                _queueSocket.Write(messageFrame);
            }
        }
    }
}
