namespace RedFoxMQ
{
    public class MessageDeserializationException : RedFoxBaseException
    {
        public MessageDeserializationException()
            : base("Error deserializing message")
        {
        }

        public MessageDeserializationException(string message)
            : base(message)
        {
        }
    }
}
