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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports
{
    class SocketFactory
    {
        public async Task<ISocket> CreateAndConnectAsync(RedFoxEndpoint endpoint)
        {
            return await CreateAndConnectAsync(endpoint, TimeSpan.FromMilliseconds(-1));
        }

        public async Task<ISocket> CreateAndConnectAsync(RedFoxEndpoint endpoint, TimeSpan timeout)
        {
            return await CreateAndConnectAsync(endpoint, timeout, new CancellationToken());
        }

        public async Task<ISocket> CreateAndConnectAsync(RedFoxEndpoint endpoint, TimeSpan timeout, CancellationToken cancellationToken)
        {
            switch (endpoint.Transport)
            {
                case RedFoxTransport.Inproc:
                    return CreateInProcSocket(endpoint);
                case RedFoxTransport.Tcp:
                    return await CreateTcpSocket(endpoint, timeout, cancellationToken);
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", endpoint.Transport));
            }
        }

        private static ISocket CreateInProcSocket(RedFoxEndpoint endpoint)
        {
            return InProcessEndpoints.Instance.Connect(endpoint);
        }

        private static async Task<ISocket> CreateTcpSocket(RedFoxEndpoint endpoint, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var tcpClient = new TcpClient { ReceiveBufferSize = 65536, SendBufferSize = 65536};
            await ConnectTcpSocketAsync(tcpClient, endpoint.Host, endpoint.Port, timeout, cancellationToken);

            return new TcpSocket(endpoint, tcpClient);
        }

        private static async Task ConnectTcpSocketAsync(TcpClient client, string hostName, int port, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
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
            }, cancellationToken);
        }
    }
}
