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
using System.Text;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageSerializationTests
    {
        [Test]
        public void no_serializer_defined_Serialize_throws_MissingMessageSerializerException()
        {
            var messageSerialization = new MessageSerialization();
            Assert.Throws<MissingMessageSerializerException>(() => messageSerialization.Serialize(new TestMessage()));
        }

        [Test]
        public void serializer_defined_but_not_for_test_message_Serialize_throws_MissingMessageSerializerException()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterSerializer(0, new TestMessageSerializer());

            Assert.Throws<MissingMessageSerializerException>(() => messageSerialization.Serialize(new TestMessage()));
        }

        [Test]
        public void default_serializer_defined_but_no_specific_message_serializer_for_test_message_use_default_serializer()
        {
            var messageSerialization = new MessageSerialization
            {
                DefaultSerializer = new TestMessageSerializer()
            };

            Assert.AreEqual(Encoding.UTF8.GetBytes("abc"), messageSerialization.Serialize(new TestMessage { Text = "abc" }));
        }

        [Test]
        public void serializer_defined_for_test_message_Serialize_returns_serialized_object()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterSerializer(TestMessage.TypeId, new TestMessageSerializer());

            Assert.AreEqual(Encoding.UTF8.GetBytes("abc"), messageSerialization.Serialize(new TestMessage { Text = "abc" }));
        }

        [Test]
        public void serializer_defined_for_message_with_MessageTypeId_65535_Serialize_returns_serialized_object()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterSerializer(CustomMessage.TypeId, new TestMessageSerializer());

            Assert.AreEqual(Encoding.UTF8.GetBytes("abc"), messageSerialization.Serialize(new CustomMessage { Text = "abc" }));
        }

        [Test]
        public void no_deserializer_defined_Deserialize_throws_MissingMessageDeserializerException()
        {
            var messageSerialization = new MessageSerialization();
            Assert.Throws<MissingMessageDeserializerException>(() => messageSerialization.Deserialize(TestMessage.TypeId, new byte[1]));
        }

        [Test]
        public void deserializer_defined_but_not_for_test_message_Deserialize_throws_MissingMessageDeserializerException()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterDeserializer(TestMessage.TypeId, new TestMessageDeserializer());

            Assert.Throws<MissingMessageDeserializerException>(() => messageSerialization.Deserialize(0, new byte[1]));
        }

        [Test]
        public void default_deserializer_defined_but_no_specific_message_deserializer_for_test_message_use_default_deserializer()
        {
            var messageSerialization = new MessageSerialization
            {
                DefaultDeserializer = new TestMessageDeserializer()
            };

            Assert.IsInstanceOf<TestMessage>(messageSerialization.Deserialize(0, new byte[1]));
        }

        [Test]
        public void deserializer_defined_for_test_message_Deserialize_creates_object()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterDeserializer(TestMessage.TypeId, new TestMessageDeserializer());

            Assert.IsInstanceOf<TestMessage>(messageSerialization.Deserialize(1, new byte[1]));
        }

        [Test]
        public void deserializer_defined_for_message_with_MessageTypeId_65535_Deserialize_creates_object()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterDeserializer(CustomMessage.TypeId, new TestMessageDeserializer());

            Assert.IsNotNull(messageSerialization.Deserialize(CustomMessage.TypeId, new byte[1]));
        }

        [Test]
        public void MessageSerializationException_should_be_thrown_on_exception_while_serializing_message()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterSerializer(ExceptionTestMessage.TypeId, new ExceptionTestMessageSerializer());

            var messageThatCausesExceptionOnSerialization = new ExceptionTestMessage(true, false);
            var exception = Assert.Throws<MessageSerializationException>(() => messageSerialization.Serialize(messageThatCausesExceptionOnSerialization));
            Assert.IsInstanceOf<TestException>(exception.InnerException);
        }

        [Test]
        public void MessageDeserializationException_should_be_thrown_on_exception_while_deserializing_message()
        {
            var messageSerialization = new MessageSerialization();
            messageSerialization.RegisterSerializer(ExceptionTestMessage.TypeId, new ExceptionTestMessageSerializer());
            messageSerialization.RegisterDeserializer(ExceptionTestMessage.TypeId, new ExceptionTestMessageDeserializer());

            var messageThatCausesExceptionOnDeserialization = new ExceptionTestMessage(false, true);
            var message = messageSerialization.Serialize(messageThatCausesExceptionOnDeserialization);
            var exception = Assert.Throws<MessageDeserializationException>(() => messageSerialization.Deserialize(ExceptionTestMessage.TypeId, message));
            Assert.IsInstanceOf<TestException>(exception.InnerException);
        }

        class CustomMessage : TestMessage
        {
            public new const ushort TypeId = ushort.MaxValue;
            public override ushort MessageTypeId { get { return TypeId; } }
        }
    }
}
