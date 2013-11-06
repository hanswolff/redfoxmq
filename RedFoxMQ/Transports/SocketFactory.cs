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
        public async Task<ISocket> CreateAndConnect(RedFoxEndpoint endpoint, int timeoutInSeconds = 0)
        {
            switch (endpoint.Transport)
            {
                case RedFoxTransport.Inproc:
                    return CreateInProcSocket(endpoint);
                case RedFoxTransport.Tcp:
                    return await CreateTcpSocket(endpoint, timeoutInSeconds);
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", endpoint.Transport));
            }
        }

        private static ISocket CreateInProcSocket(RedFoxEndpoint endpoint)
        {
            var queueStream = InProcessEndpoints.Instance.Connect(endpoint);
            return new InProcSocket(endpoint, queueStream);
        }

        private static async Task<ISocket> CreateTcpSocket(RedFoxEndpoint endpoint, int timeoutInSeconds)
        {
            var tcpClient = new TcpClient { ReceiveBufferSize = 65536, SendBufferSize = 65536};
            await ConnecTcpSocketAsync(tcpClient, endpoint.Host, endpoint.Port, timeoutInSeconds);

            return new TcpSocket(endpoint, tcpClient);
        }

        private static async Task ConnecTcpSocketAsync(TcpClient client, string hostName, int port, int timeoutInSeconds)
        {
            var tcs = new TaskCompletionSource<bool>();

            Timer timer = null;
            if (timeoutInSeconds != 0)
            {
                timer = new Timer(
                    t => tcs.TrySetException(new TimeoutException(String.Format("Timeout occured trying to connect to {0}:{1}", hostName, port))),
                    null, TimeSpan.FromSeconds(timeoutInSeconds), TimeSpan.FromMilliseconds(-1));
            }

            var task = Task.Factory.FromAsync(
                client.BeginConnect,
                ar =>
                {
                    if (timer != null) timer.Dispose();
                    if (tcs.TrySetResult(true))
                    {
                        try
                        {
                            client.EndConnect(ar);
                        }
                        catch (ObjectDisposedException) { }
                    }

                },
                hostName, port, null);

            await tcs.Task;
        }
    }
}
