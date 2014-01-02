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
    class MessageFrameQueueReader : IMessageFrameReader
    {
        private readonly IQueueSocket _queueSocket;

        public MessageFrameQueueReader(IQueueSocket queueSocket)
        {
            if (queueSocket == null) throw new ArgumentNullException("queueSocket");
            _queueSocket = queueSocket;
        }

        public async Task<MessageFrame> ReadMessageFrameAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(() => _queueSocket.Read(cancellationToken), cancellationToken);
        }

        public MessageFrame ReadMessageFrame()
        {
            return _queueSocket.Read();
        }
    }
}
