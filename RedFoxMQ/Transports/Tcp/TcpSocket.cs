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
using System.IO;
using System.Net.Sockets;

namespace RedFoxMQ.Transports.Tcp
{
    public class TcpSocket : ISocket
    {
        private readonly TcpClient _tcpClient;

        private readonly Stream _stream;
        public Stream Stream
        {
            get { return _stream; }
        }

        public RedFoxEndpoint Endpoint { get; private set; }

        public TcpSocket(RedFoxEndpoint endpoint, TcpClient tcpClient)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            _tcpClient = tcpClient;

            Endpoint = endpoint;
            _stream = tcpClient.GetStream();
        }

        private volatile bool _isDisconnected;
        public bool IsDisconnected
        {
            get { return _isDisconnected; }
        }

        public event Action Disconnected = () => { };

        public void Disconnect()
        {
            // TODO: fix race condition
            if (_isDisconnected) return;
            _isDisconnected = true;

            _tcpClient.Close();
            Disconnected();
        }
    }
}
