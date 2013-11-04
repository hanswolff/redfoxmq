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
using NUnit.Framework;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;

namespace RedFoxMQ.Tests.Transports
{
    [TestFixture]
    public class SocketAccepterFactoryTests
    {
        [Test]
        public void InProcessTransport_creates_InProcessBroadcastSocketAccepter()
        {
            var accepter = new SocketAccepterFactory().CreateForTransport(RedFoxTransport.Inproc);
            Assert.IsInstanceOf<InProcessSocketAccepter>(accepter);
        }

        [Test]
        public void TcpTransport_creates_TcpClientBroadcastSocketAccepter()
        {
            var accepter = new SocketAccepterFactory().CreateForTransport(RedFoxTransport.Tcp);
            Assert.IsInstanceOf<TcpSocketAccepter>(accepter);
        }
    }
}
