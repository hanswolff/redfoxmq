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

            return serializer.Serialize(message);
        }

        public IMessage Deserialize(ushort messageTypeId, byte[] serializedMessage)
        {
            if (serializedMessage == null) throw new ArgumentNullException("serializedMessage");

            var deserializer = _deserializers[messageTypeId];
            if (deserializer == null)
            {
                throw new MissingMessageDeserializerException(messageTypeId);
            }

            return deserializer.Deserialize(serializedMessage);
        }
    }
}
