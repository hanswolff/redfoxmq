using System;

namespace RedFoxMQ
{
    class MessageFrame
    {
        public ushort MessageTypeId;
        public byte[] RawMessage;

        public DateTime TimestampSent;
        public DateTime TimestampReceived;
    }
}
