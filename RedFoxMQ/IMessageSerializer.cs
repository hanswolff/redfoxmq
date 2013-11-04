namespace RedFoxMQ
{
    public interface IMessageSerializer
    {
        byte[] Serialize(IMessage message);
    }
}
