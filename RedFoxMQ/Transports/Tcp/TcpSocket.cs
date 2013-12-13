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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.Tcp
{
    public class TcpSocket : ISocket
    {
        private readonly TcpClient _tcpClient;

        private readonly NetworkStream _stream;

        public RedFoxEndpoint Endpoint { get; private set; }

        private readonly System.Net.EndPoint _remoteEndPoint;
        public TcpSocket(RedFoxEndpoint endpoint, TcpClient tcpClient)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            _tcpClient = tcpClient;
            _remoteEndPoint = tcpClient.Client.RemoteEndPoint;

            Endpoint = endpoint;
            _stream = tcpClient.GetStream();
        }

        private readonly InterlockedBoolean _isDisconnected = new InterlockedBoolean();
        public bool IsDisconnected
        {
            get { return _isDisconnected.Value; }
        }

        public event Action Disconnected = () => { };

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

        public void Disconnect()
        {
            if (_isDisconnected.Set(true)) return;

            _tcpClient.Close();
            Disconnected();
        }

        public override string ToString()
        {
            return GetType().Name + ", RemoteEndpoint: " + _remoteEndPoint;
        }
    }
}
