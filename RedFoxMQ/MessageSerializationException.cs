namespace RedFoxMQ
{
    public class MessageSerializationException : RedFoxBaseException
    {
        public MessageSerializationException()
            : base("Error serializing message")
        {
        }

        public MessageSerializationException(string message)
            : base(message)
        {
        }
    }
}
