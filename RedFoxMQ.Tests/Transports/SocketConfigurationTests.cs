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
using System;

namespace RedFoxMQ.Tests.Transports
{
    [TestFixture]
    public class SocketConfigurationTests
    {
        [Test]
        public void Clone()
        {
            var socketConfiguration = new SocketConfiguration
            {
                ConnectTimeout = TimeSpan.FromSeconds(1),
                ReceiveTimeout = TimeSpan.FromSeconds(2),
                SendTimeout = TimeSpan.FromSeconds(3),
                ReceiveBufferSize = 4,
                SendBufferSize = 5
            };

            var cloned = socketConfiguration.Clone();

            Assert.AreNotSame(socketConfiguration, cloned);

            Assert.AreEqual(socketConfiguration.ConnectTimeout, cloned.ConnectTimeout);
            Assert.AreEqual(socketConfiguration.ReceiveTimeout, cloned.ReceiveTimeout);
            Assert.AreEqual(socketConfiguration.SendTimeout, cloned.SendTimeout);
            Assert.AreEqual(socketConfiguration.ReceiveBufferSize, cloned.ReceiveBufferSize);
            Assert.AreEqual(socketConfiguration.SendBufferSize, cloned.SendBufferSize);
        }
    }
}
