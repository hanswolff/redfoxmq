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
using RedFoxMQ.Tests.TestHelpers;
using System.Text;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageSerializationTests
    {
        public const ushort CorrectMessageTypeId = 1;

        [Test]
        public void no_serializer_defined_Serialize_throws_MissingMessageSerializerException()
        {
            MessageSerialization.Instance.RemoveAllSerializers();
            Assert.Throws<MissingMessageSerializerException>(() => MessageSerialization.Instance.Serialize(new TestMessage()));
        }

        [Test]
        public void serializer_defined_but_not_for_test_message_Serialize_throws_MissingMessageSerializerException()
        {
            MessageSerialization.Instance.RemoveAllSerializers();
            MessageSerialization.Instance.RegisterSerializer(0, new TestMessageSerializer());

            Assert.Throws<MissingMessageSerializerException>(() => MessageSerialization.Instance.Serialize(new TestMessage()));
        }

        [Test]
        public void serializer_defined_for_test_message_Serialize_returns_serialized_object()
        {
            MessageSerialization.Instance.RemoveAllSerializers();
            MessageSerialization.Instance.RegisterSerializer(CorrectMessageTypeId, new TestMessageSerializer());

            Assert.AreEqual(Encoding.UTF8.GetBytes("abc"), MessageSerialization.Instance.Serialize(new TestMessage { Text = "abc"}));
        }

        [Test]
        public void RemoveAllSerializers_removes_serializers()
        {
            MessageSerialization.Instance.RegisterSerializer(CorrectMessageTypeId, new TestMessageSerializer());
            MessageSerialization.Instance.RemoveAllSerializers();

            Assert.Throws<MissingMessageSerializerException>(() => MessageSerialization.Instance.Serialize(new TestMessage()));
        }

        [Test]
        public void no_deserializer_defined_Deserialize_throws_MissingMessageDeserializerException()
        {
            MessageSerialization.Instance.RemoveAllDeserializers();
            Assert.Throws<MissingMessageDeserializerException>(() => MessageSerialization.Instance.Deserialize(CorrectMessageTypeId, new byte[1]));
        }

        [Test]
        public void deserializer_defined_but_not_for_test_message_Deserialize_throws_MissingMessageDeserializerException()
        {
            MessageSerialization.Instance.RemoveAllDeserializers();
            MessageSerialization.Instance.RegisterDeserializer(0, new TestMessageDeserializer());

            Assert.Throws<MissingMessageDeserializerException>(() => MessageSerialization.Instance.Deserialize(CorrectMessageTypeId, new byte[1]));
        }

        [Test]
        public void deserializer_defined_for_test_message_Deserialize_creates_object()
        {
            MessageSerialization.Instance.RemoveAllDeserializers();
            MessageSerialization.Instance.RegisterDeserializer(CorrectMessageTypeId, new TestMessageDeserializer());

            Assert.IsInstanceOf<TestMessage>(MessageSerialization.Instance.Deserialize(1, new byte[1]));
        }

        [Test]
        public void RemoveAllDeserializers_removes_serializers()
        {
            MessageSerialization.Instance.RegisterDeserializer(CorrectMessageTypeId, new TestMessageDeserializer());
            MessageSerialization.Instance.RemoveAllDeserializers();

            Assert.Throws<MissingMessageDeserializerException>(() => MessageSerialization.Instance.Deserialize(CorrectMessageTypeId, new byte[1]));
        }
    }
}
