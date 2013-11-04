using System;

namespace RedFoxMQ
{
    public class MissingMessageDeserializerException : RedFoxBaseException
    {
        public MissingMessageDeserializerException()
            : base(String.Format("No message deserializer defined for message"))
        {
        }

        public MissingMessageDeserializerException(int messageTypeId)
            : base(String.Format("No message deserializer defined for message type ID {0}", messageTypeId))
        {
        }
    }
}
