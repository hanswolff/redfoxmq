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
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.InProc
{
    class InProcSocket : ISocket
    {
        public RedFoxEndpoint Endpoint { get; private set; }
        
        private readonly QueueStream _writeStream;
        public QueueStream WriteStream { get { return _writeStream; } }

        private readonly QueueStream _readStream;
        public QueueStream ReadStream { get { return _readStream; } }

        public InProcSocket(RedFoxEndpoint endpoint, QueueStream writeStream, QueueStream readStream)
        {
            if (writeStream == null) throw new ArgumentNullException("writeStream");
            if (readStream == null) throw new ArgumentNullException("readStream");
            Endpoint = endpoint;
            _writeStream = writeStream;
            _readStream = readStream;
        }

        private readonly InterlockedBoolean _isDisconnected = new InterlockedBoolean();
        public bool IsDisconnected
        {
            get { return _isDisconnected.Value; }
        }

        public event DisconnectedDelegate Disconnected = () => { };

        public int Read(byte[] buf, int offset, int count)
        {
            return _readStream.Read(buf, offset, count);
        }

        public async Task<int> ReadAsync(byte[] buf, int offset, int count, CancellationToken cancellationToken)
        {
            return await _readStream.ReadAsync(buf, offset, count, cancellationToken);
        }

        public void Write(byte[] buf, int offset, int count)
        {
            _writeStream.Write(buf, offset, count);
        }

        public async Task WriteAsync(byte[] buf, int offset, int count, CancellationToken cancellationToken)
        {
            await _writeStream.WriteAsync(buf, offset, count, cancellationToken);
        }

        public void Disconnect()
        {
            if (_isDisconnected.Set(true)) return;

            _readStream.Close();
            _writeStream.Close();

            Disconnected();
        }

        public override string ToString()
        {
            return GetType().Name + ", " + Endpoint;
        }
    }
}
