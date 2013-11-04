namespace RedFoxMQ
{
    public interface IMessageDeserializer
    {
        IMessage Deserialize(byte[] rawMessage);
    }
}
