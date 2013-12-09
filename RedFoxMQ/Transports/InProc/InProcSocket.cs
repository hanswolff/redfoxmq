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

namespace RedFoxMQ.Transports.InProc
{
    class InProcSocket : ISocket
    {
        public RedFoxEndpoint Endpoint { get; private set; }
        public Stream Stream { get; private set; }

        public InProcSocket(RedFoxEndpoint endpoint, QueueStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Endpoint = endpoint;
            Stream = stream;
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

            Disconnected();
        }
    }
}
