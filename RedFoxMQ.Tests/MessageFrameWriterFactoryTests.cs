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

using Moq;
using NUnit.Framework;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;
using System;
using System.Net;
using System.Net.Sockets;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageFrameWriterFactoryTests
    {
        [Test]
        public void UnknownSocket_throws_NotSupportedxception()
        {
            var socket = new Mock<ISocket>(MockBehavior.Loose).Object;

            var factory = new MessageFrameWriterFactory();
            Assert.Throws<NotSupportedException>(() => factory.CreateWriterFromSocket(socket));
        }

        [Test]
        public void InProcSocket_maps_to_MessageFrameQueueWriter()
        {
            var socket = new InProcSocket();

            var factory = new MessageFrameWriterFactory();
            var writer = factory.CreateWriterFromSocket(socket);

            Assert.IsInstanceOf<MessageFrameQueueWriter>(writer);
        }

        [Test]
        public void TcpSocket_maps_to_MessageFrameQueueWriter()
        {
            var port = TestHelpers.GetFreePort();
            
            var server = new TcpListener(IPAddress.Loopback, port);
            server.Start();
            server.AcceptSocketAsync();
            
            using (var client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, port);
                var socket = new TcpSocket(new RedFoxEndpoint(), client);

                var factory = new MessageFrameWriterFactory();
                var writer = factory.CreateWriterFromSocket(socket);

                Assert.IsInstanceOf<MessageFrameStreamWriter>(writer);
            }
        }
    }
}
