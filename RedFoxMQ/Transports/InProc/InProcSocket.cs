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

using System;
using System.Threading;

namespace RedFoxMQ.Transports.InProc
{
    class InProcSocket : IQueueSocket
    {
        public RedFoxEndpoint Endpoint { get; private set; }
        
        private readonly BlockingConcurrentQueue<MessageFrame> _writeStream;
        public BlockingConcurrentQueue<MessageFrame> WriteStream { get { return _writeStream; } }

        private readonly BlockingConcurrentQueue<MessageFrame> _readStream;
        public BlockingConcurrentQueue<MessageFrame> ReadStream { get { return _readStream; } }

        public InProcSocket(RedFoxEndpoint endpoint, BlockingConcurrentQueue<MessageFrame> writeStream, BlockingConcurrentQueue<MessageFrame> readStream)
        {
            if (writeStream == null) throw new ArgumentNullException("writeStream");
            if (readStream == null) throw new ArgumentNullException("readStream");
            Endpoint = endpoint;
            _writeStream = writeStream;
            _readStream = readStream;
        }

        private readonly CancellationTokenSource _ctsDisconnect = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private readonly InterlockedBoolean _isDisconnected = new InterlockedBoolean();
        public bool IsDisconnected
        {
            get { return _isDisconnected.Value; }
        }

        public event DisconnectedDelegate Disconnected;

        public InProcSocket()
        {
            _cancellationToken = _ctsDisconnect.Token;
            Disconnected += () => _ctsDisconnect.Cancel();
        }

        public MessageFrame Read()
        {
            return _readStream.Dequeue(_cancellationToken);
        }

        public MessageFrame Read(CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken))
            {
                return _readStream.Dequeue(cts.Token);
            }
        }

        public void Write(MessageFrame messageFrame)
        {
            _writeStream.Enqueue(messageFrame);
        }

        public void Disconnect()
        {
            if (_isDisconnected.Set(true)) return;

            Disconnected();
        }

        public override string ToString()
        {
            return GetType().Name + ", " + Endpoint;
        }
    }
}
