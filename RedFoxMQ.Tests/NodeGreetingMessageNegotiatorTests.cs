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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class NodeGreetingMessageNegotiatorTests
    {
        [Test]
        public void WriteGreeting_writes_full_NodeGreetingMessage()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                negotiator.WriteGreeting(message);

                Assert.AreEqual(message.Serialize(), mem.ToArray());
            }
        }

        [Test]
        public void WriteGreetingAsync_writes_full_NodeGreetingMessage()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                negotiator.WriteGreetingAsync(message, CancellationToken.None).Wait();

                Assert.AreEqual(message.Serialize(), mem.ToArray());
            }
        }

        [Test]
        public void VerifyRemoteGreeting_matching_expected_NodeType()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                mem.Write(message.Serialize(), 0, message.Serialize().Length);
                mem.Position = 0;

                negotiator.VerifyRemoteGreeting(NodeType.Responder);
            }
        }

        [Test]
        public void VerifyRemoteGreeting_not_matching_expected_NodeType()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                mem.Write(message.Serialize(), 0, message.Serialize().Length);
                mem.Position = 0;

                Assert.Throws<RedFoxProtocolException>(() => negotiator.VerifyRemoteGreeting(NodeType.Requester));
            }
        }

        [Test]
        public void VerifyRemoteGreetingAsync_matching_expected_NodeType()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                mem.Write(message.Serialize(), 0, message.Serialize().Length);
                mem.Position = 0;

                negotiator.VerifyRemoteGreetingAsync(NodeType.Responder, CancellationToken.None).Wait();
            }
        }

        [Test]
        public async Task VerifyRemoteGreetingAsync_not_matching_expected_NodeType()
        {
            using (var mem = new MemoryStream())
            using (var socket = new TestStreamSocket(mem))
            {
                var negotiator = new NodeGreetingMessageNegotiator(socket);

                var message = new NodeGreetingMessage(NodeType.Responder);
                mem.Write(message.Serialize(), 0, message.Serialize().Length);
                mem.Position = 0;

                try
                {
                    await negotiator.VerifyRemoteGreetingAsync(NodeType.Requester, CancellationToken.None);
                    Assert.Fail();
                }
                catch (RedFoxProtocolException)
                {
                    Assert.Pass();
                }
            }
        }
    }
}
