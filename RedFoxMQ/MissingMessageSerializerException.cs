using System;

namespace RedFoxMQ
{
    public class MissingMessageSerializerException : RedFoxBaseException
    {
        public MissingMessageSerializerException()
            : base(String.Format("No message serializer defined for message"))
        {
        }

        public MissingMessageSerializerException(int messageTypeId)
            : base(String.Format("No message serializer defined for message type ID {0}", messageTypeId))
        {
        }

        public MissingMessageSerializerException(int messageTypeId, Type messageType)
            : base(String.Format("No message serializer defined for message type ID {0} (message type: {1})", messageTypeId, messageType))
        {
        }
    }
}
