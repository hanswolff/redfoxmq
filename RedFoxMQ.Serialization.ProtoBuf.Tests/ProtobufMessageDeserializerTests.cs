// 
// Copyright 2014 Hans Wolff
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

using System;
using System.Reflection;
using NUnit.Framework;
using ProtoBuf;
using System.IO;

namespace RedFoxMQ.Serialization.ProtoBuf.Tests
{
    [TestFixture]
    public class ProtobufMessageDeserializerTests
    {
        [Test]
        public void SetDeserializer_GetDeserializer()
        {
            var deserializer = new ProtobufMessageDeserializer();
            Func<Stream, IMessage> deserializerFunc = Serializer.Deserialize<TestMessage>;
            deserializer.SetDeserializer(1, deserializerFunc);
            Assert.AreEqual(deserializerFunc, deserializer.GetDeserializer(1));
        }

        [Test]
        public void Deserialize_existing_MessageTypeId()
        {
            var deserializer = new ProtobufMessageDeserializer();
            deserializer.SetDeserializer(1, Serializer.Deserialize<TestMessage>);

            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write((ushort)1);
                mem.Position = 0;

                var message = deserializer.Deserialize(mem.ToArray());
                Assert.AreEqual(typeof(TestMessage), message.GetType());
            }
        }

        [Test]
        public void Deserialize_non_mapped_MessageTypeId_throws_MessageDeserializationException()
        {
            var deserializer = new ProtobufMessageDeserializer();

            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write((ushort)1);
                mem.Position = 0;

                Assert.Throws<MessageDeserializationException>(() => deserializer.Deserialize(mem.ToArray()));
            }
        }

        [Test]
        public void AssignAssemblyMessageTypes()
        {
            var deserializer = new ProtobufMessageDeserializer();
            deserializer.AssignAssemblyMessageTypes(Assembly.GetExecutingAssembly());

            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write((ushort)1);
                mem.Position = 0;

                var message = deserializer.Deserialize(mem.ToArray());
                Assert.AreEqual(typeof(TestMessage), message.GetType());
            }
        }
    }
}
