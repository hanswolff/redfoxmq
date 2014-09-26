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

using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;
using System;
using System.Net.Sockets;

namespace RedFoxMQ.Transports
{
    class SocketFactory
    {
        private static readonly NodeTypeHasReceiveTimeout NodeTypeHasReceiveTimeout = new NodeTypeHasReceiveTimeout();

        public ISocket CreateAndConnectAsync(RedFoxEndpoint endpoint, NodeType nodeType, ISocketConfiguration socketConfiguration)
        {
            switch (endpoint.Transport)
            {
                case RedFoxTransport.Inproc:
                    return CreateInProcSocket(endpoint);
                case RedFoxTransport.Tcp:
                    return CreateTcpSocket(endpoint, nodeType, socketConfiguration);
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", endpoint.Transport));
            }
        }

        private static ISocket CreateInProcSocket(RedFoxEndpoint endpoint)
        {
            return InProcessEndpoints.Instance.Connect(endpoint);
        }

        private static ISocket CreateTcpSocket(RedFoxEndpoint endpoint, NodeType nodeType, ISocketConfiguration socketConfiguration)
        {
            var hasReceiveTimeout = NodeTypeHasReceiveTimeout.HasReceiveTimeout(nodeType);

            var tcpClient = new TcpClient
            {
                ReceiveTimeout = hasReceiveTimeout ? socketConfiguration.ReceiveTimeout.ToMillisOrZero() : 0,
                SendTimeout = socketConfiguration.SendTimeout.ToMillisOrZero(),

                NoDelay = true,
                ReceiveBufferSize = socketConfiguration.ReceiveBufferSize,
                SendBufferSize = socketConfiguration.SendBufferSize
            };
            ConnectTcpSocket(tcpClient, endpoint.Host, endpoint.Port, socketConfiguration.ConnectTimeout);

            return new TcpSocket(endpoint, tcpClient);
        }

        private static void ConnectTcpSocket(TcpClient client, string hostName, int port, TimeSpan timeout)
        {
            var ar = client.BeginConnect(hostName, port, null, null);
            var wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(timeout, false))
                {
                    client.Close();
                    throw new TimeoutException();
                }

                client.EndConnect(ar);
            }
            finally
            {
                wh.Close();
            }
        }
    }
}
