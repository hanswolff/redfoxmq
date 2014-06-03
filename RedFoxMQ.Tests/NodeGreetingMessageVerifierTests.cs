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

using NUnit.Framework;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.Tcp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class NodeGreetingMessageVerifierTests
    {
        [Test]
        public void SendReceiveAndVerify_times_out_after_ConnectTimeout()
        {
            var endpoint = TestHelpers.CreateEndpointForTransport(RedFoxTransport.Tcp);
            var socketConfiguration = new SocketConfiguration
            {
                ConnectTimeout = TimeSpan.FromSeconds(2)
            };

            var server = new TcpListener(IPAddress.Loopback, endpoint.Port);
            server.Start();
            try
            {
                server.AcceptTcpClientAsync();

                var nodeGreetingVerifier = new NodeGreetingMessageVerifier(NodeType.Publisher, NodeType.Subscriber);

                var socket = new SocketFactory().CreateAndConnectAsync(endpoint, socketConfiguration);

                var sw = Stopwatch.StartNew();
                var task = Task.Factory.StartNew(() =>
                {
                    nodeGreetingVerifier.SendReceiveAndVerify(socket, socketConfiguration.ConnectTimeout);
                });

                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    task.Wait(cancellationToken.Token);
                }
                catch (AggregateException ex)
                {
                    Assert.AreEqual(typeof(TimeoutException), ex.InnerExceptions.Single().GetType());
                }
                Assert.Greater(sw.Elapsed, socketConfiguration.ConnectTimeout);
            }
            finally
            {
                server.Stop();
            }
        }

        [Test]
        public void SendReceiveAndVerify_keeps_SendTimeout_ReceiveTimeout()
        {
            var endpoint = TestHelpers.CreateEndpointForTransport(RedFoxTransport.Tcp);
            var socketConfiguration = new SocketConfiguration
            {
                SendTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(20)
            };

            var server = new TcpListener(IPAddress.Loopback, endpoint.Port);
            server.Start();
            try
            {
                server.AcceptTcpClientAsync();

                var nodeGreetingVerifier = new NodeGreetingMessageVerifier(NodeType.Publisher, NodeType.Subscriber);

                var socket = (TcpSocket)new SocketFactory().CreateAndConnectAsync(endpoint, socketConfiguration);
                Assert.Throws<TimeoutException>(() => nodeGreetingVerifier.SendReceiveAndVerify(socket, TimeSpan.FromSeconds(2)));

                Assert.AreEqual(socketConfiguration.SendTimeout.ToMillisOrZero(), socket.TcpClient.SendTimeout);
                Assert.AreEqual(socketConfiguration.ReceiveTimeout.ToMillisOrZero(), socket.TcpClient.ReceiveTimeout);
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
