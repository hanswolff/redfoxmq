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
using System.Net;
using System.Net.Sockets;

namespace RedFoxMQ.Tests.Transports
{
    [TestFixture]
    public class SocketFactoryTests
    {
        [TestCase(NodeType.Publisher, false)]
        [TestCase(NodeType.Requester, true)]
        [TestCase(NodeType.Responder, true)]
        [TestCase(NodeType.ServiceQueue, false)]
        [TestCase(NodeType.ServiceQueueReader, true)]
        [TestCase(NodeType.ServiceQueueWriter, true)]
        [TestCase(NodeType.Subscriber, false)]
        public void SocketConfiguration_applied_to_TcpSocket_when_connecting(NodeType nodeType, bool hasReceiveTimeout)
        {
            var endpoint = TestHelpers.CreateEndpointForTransport(RedFoxTransport.Tcp);
            var socketConfiguration = new SocketConfiguration
            {
                ConnectTimeout = TimeSpan.FromSeconds(5),
                SendBufferSize = 128,
                ReceiveBufferSize = 256,
                SendTimeout = TimeSpan.FromSeconds(1),
                ReceiveTimeout = TimeSpan.FromSeconds(2)
            };

            var server = new TcpListener(IPAddress.Loopback, endpoint.Port);
            server.Start();
            try
            {
                server.AcceptTcpClientAsync();

                var socket = (TcpSocket)new SocketFactory().CreateAndConnectAsync(endpoint, nodeType, socketConfiguration);

                Assert.AreEqual(socketConfiguration.SendBufferSize, socket.TcpClient.SendBufferSize);
                Assert.AreEqual(socketConfiguration.ReceiveBufferSize, socket.TcpClient.ReceiveBufferSize);
                Assert.AreEqual(socketConfiguration.SendTimeout.ToMillisOrZero(), socket.TcpClient.SendTimeout);
                var expectedReceiveTimeout = hasReceiveTimeout ? socketConfiguration.ReceiveTimeout.ToMillisOrZero() : 0;
                Assert.AreEqual(expectedReceiveTimeout, socket.TcpClient.ReceiveTimeout);
            }
            finally
            {
                server.Stop();
            }
        }

    }
}
