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
using System;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class NodeGreetingMessageTests
    {
        [Test]
        public void Serialize_message_size()
        {
            var greetingMessage = new NodeGreetingMessage(NodeType.Publisher);
            var serializedMessage = greetingMessage.Serialize();

            const int expectedSize = 3;
            Assert.AreEqual(expectedSize, serializedMessage.Length);
            Assert.AreEqual(expectedSize - 1, serializedMessage[0]);
        }

        [Test]
        public void Serialize_ProtocolVersion()
        {
            var greetingMessage = new NodeGreetingMessage(NodeType.Publisher);
            var serializedMessage = greetingMessage.Serialize();

            Assert.AreEqual(1, serializedMessage[1]);
        }

        [Test]
        public void Serialize_NodeType()
        {
            var greetingMessage = new NodeGreetingMessage(NodeType.Subscriber);
            var serializedMessage = greetingMessage.Serialize();

            Assert.AreEqual((byte)NodeType.Subscriber, serializedMessage[2]);
        }

        [Test]
        public void Deserialize_Null()
        {
            Assert.Throws<ArgumentNullException>(() => NodeGreetingMessage.DeserializeWithoutLength(null));
        }

        [Test]
        public void Deserialize_TooSmall()
        {
            Assert.Throws<RedFoxProtocolException>(() => NodeGreetingMessage.DeserializeWithoutLength(new byte[] { 1 }));
        }

        [Test]
        public void Deserialize_InvalidProtocol()
        {
            Assert.Throws<RedFoxProtocolException>(() => NodeGreetingMessage.DeserializeWithoutLength(new byte[] { 2, 1 }));
        }

        [Test]
        public void Deserialize_NoteType_Success()
        {
            var originalMessage = new NodeGreetingMessage(NodeType.Subscriber);
            var serialized = originalMessage.Serialize();
            var deserializedMessage = NodeGreetingMessage.DeserializeWithoutLength(serialized, 1);

            Assert.AreEqual(originalMessage.NodeType, deserializedMessage.NodeType);
        }
    }
}
