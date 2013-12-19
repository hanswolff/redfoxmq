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

using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;
using System;

namespace RedFoxMQ.Transports
{
    class SocketAccepterFactory
    {
        public ISocketAccepter CreateForTransport(RedFoxTransport transport)
        {
            switch (transport)
            {
                case RedFoxTransport.Inproc:
                    return new InProcessSocketAccepter();
                case RedFoxTransport.Tcp:
                    return new TcpSocketAccepter();
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", transport));
            }
        }

        public ISocketAccepter CreateAndBind(RedFoxEndpoint endpoint,
            ISocketConfiguration socketConfiguration,
            SocketMode socketMode = SocketMode.ReadWrite, 
            Action<ISocket, ISocketConfiguration> onClientConnected = null, 
            Action<ISocket> onClientDisconnected = null)
        {
            if (socketConfiguration == null) throw new ArgumentNullException("socketConfiguration");

            var server = CreateForTransport(endpoint.Transport);
            server.Bind(endpoint, socketConfiguration, socketMode, onClientConnected, onClientDisconnected);
            return server;
        }
    }
}
