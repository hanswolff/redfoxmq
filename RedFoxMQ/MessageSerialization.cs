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

using System;

namespace RedFoxMQ
{
    public sealed class MessageSerialization : IMessageSerialization
    {
        private readonly IMessageSerializer[] _serializers = new IMessageSerializer[ushort.MaxValue + 1];
        private readonly IMessageDeserializer[] _deserializers = new IMessageDeserializer[ushort.MaxValue + 1];

        public IMessageSerializer DefaultSerializer { get; set; }
        public IMessageDeserializer DefaultDeserializer { get; set; }

        public void RegisterSerializer(ushort messageTypeId, IMessageSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializers[messageTypeId] = serializer;
        }

        public void RegisterDeserializer(ushort messageTypeId, IMessageDeserializer deserializer)
        {
            if (deserializer == null) throw new ArgumentNullException("deserializer");
            _deserializers[messageTypeId] = deserializer;
        }

        public byte[] Serialize(IMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var messageTypeId = message.MessageTypeId;

            var serializer = _serializers[messageTypeId] ?? DefaultSerializer;
            if (serializer == null)
            {
                throw new MissingMessageSerializerException(messageTypeId, message.GetType());
            }

            try
            {
                return serializer.Serialize(message);
            }
            catch (Exception ex)
            {
                throw new MessageSerializationException(ex);
            }
        }

        public IMessage Deserialize(ushort messageTypeId, byte[] serializedMessage)
        {
            if (serializedMessage == null) throw new ArgumentNullException("serializedMessage");

            var deserializer = _deserializers[messageTypeId] ?? DefaultDeserializer;
            if (deserializer == null)
            {
                throw new MissingMessageDeserializerException(messageTypeId);
            }

            try
            {
                return deserializer.Deserialize(serializedMessage);
            }
            catch (Exception ex)
            {
                throw new MessageDeserializationException(ex);
            }
        }
    }
}
