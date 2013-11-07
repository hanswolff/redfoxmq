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
using System;

namespace RedFoxMQ
{
    public class MessageSerialization
    {
        private static volatile MessageSerialization _instance;

        public static MessageSerialization Instance
        {
            get
            {
                if (_instance != null) return _instance;
                return _instance = new MessageSerialization();
            }
        }

        private MessageSerialization()
        {
        }

        private readonly IMessageSerializer[] _serializers = new IMessageSerializer[ushort.MaxValue];
        private readonly IMessageDeserializer[] _deserializers = new IMessageDeserializer[ushort.MaxValue];

        public void RemoveAllSerializers()
        {
            for (var i = 0; i < _serializers.Length; i++)
            {
                _serializers[i] = null;
            }
        }

        public void RemoveAllDeserializers()
        {
            for (var i = 0; i < _deserializers.Length; i++)
            {
                _deserializers[i] = null;
            }
        }

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

            var serializer = _serializers[messageTypeId];
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

            var deserializer = _deserializers[messageTypeId];
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
