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

using System.Collections.Generic;
using NUnit.Framework;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.InProc;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class NodeGreetingMessageQueueSocketNegotiatorTests
    {
        [Test]
        public void WriteGreeting_writes_full_NodeGreetingMessage()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);

            var message = new NodeGreetingMessage(NodeType.Responder);
            negotiator.WriteGreeting(message);

            Assert.AreEqual(message.Serialize(), blockingQueue.Dequeue(CancellationToken.None).RawMessage);
        }

        [Test]
        public void WriteGreetingAsync_writes_full_NodeGreetingMessage()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);

            var message = new NodeGreetingMessage(NodeType.Responder);
            negotiator.WriteGreetingAsync(message, CancellationToken.None).Wait();

            Assert.AreEqual(message.Serialize(), blockingQueue.Dequeue(CancellationToken.None).RawMessage);
        }

        [Test]
        public void VerifyRemoteGreeting_matching_expected_NodeType()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);
            var message = new NodeGreetingMessage(NodeType.Responder);
            blockingQueue.Enqueue(new MessageFrame { RawMessage = message.Serialize() });

            negotiator.VerifyRemoteGreeting(new HashSet<NodeType> { NodeType.Responder });
        }

        [Test]
        public void VerifyRemoteGreeting_not_matching_expected_NodeType()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);
            var message = new NodeGreetingMessage(NodeType.Responder);
            blockingQueue.Enqueue(new MessageFrame { RawMessage = message.Serialize() });

            Assert.Throws<RedFoxProtocolException>(() => new HashSet<NodeType> { NodeType.Requester });
        }

        [Test]
        public void VerifyRemoteGreetingAsync_matching_expected_NodeType()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);
            var message = new NodeGreetingMessage(NodeType.Responder);
            blockingQueue.Enqueue(new MessageFrame { RawMessage = message.Serialize() });

            negotiator.VerifyRemoteGreetingAsync(new HashSet<NodeType> { NodeType.Responder }, CancellationToken.None).Wait();
        }

        [Test]
        public async Task VerifyRemoteGreetingAsync_not_matching_expected_NodeType()
        {
            var blockingQueue = new BlockingConcurrentQueue<MessageFrame>();
            var socket = new InProcSocket(RedFoxEndpoint.Empty, blockingQueue, blockingQueue);

            var negotiator = new NodeGreetingMessageQueueSocketNegotiator(socket);
            var message = new NodeGreetingMessage(NodeType.Responder);
            blockingQueue.Enqueue(new MessageFrame { RawMessage = message.Serialize() });

            try
            {
                await negotiator.VerifyRemoteGreetingAsync(new HashSet<NodeType> { NodeType.Requester }, CancellationToken.None);
                Assert.Fail();
            }
            catch (RedFoxProtocolException)
            {
                Assert.Pass();
            }
        }
    }
}
