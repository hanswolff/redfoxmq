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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RedFoxMQ.Tests
{
    sealed class TestSocket : ISocket, IDisposable
    {
        private readonly Stream _stream;

        public TestSocket(Stream stream)
        {
            _stream = stream;
        }

        public bool IsDisconnected { get { return _isDisconnected.Value; }}

        private readonly InterlockedBoolean _isDisconnected = new InterlockedBoolean();
        public event DisconnectedDelegate Disconnected = () => { };

        public void Disconnect()
        {
            if (_isDisconnected.Set(true)) return;

            _stream.Close();

            Disconnected();
        }

        public RedFoxEndpoint Endpoint { get; private set; }

        public int Read(byte[] buf, int offset, int count)
        {
            return _stream.Read(buf, offset, count);
        }

        public async Task<int> ReadAsync(byte[] buf, int offset, int count, CancellationToken cancellationToken)
        {
            return await _stream.ReadAsync(buf, offset, count, cancellationToken);
        }

        public void Write(byte[] buf, int offset, int count)
        {
            _stream.Write(buf, offset, count);
        }

        public async Task WriteAsync(byte[] buf, int offset, int count, CancellationToken cancellationToken)
        {
            await _stream.WriteAsync(buf, offset, count, cancellationToken);
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
