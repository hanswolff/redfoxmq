using System.Text;

namespace RedFoxMQ.Tests.TestHelpers
{
    class TestMessageDeserializer : IMessageDeserializer
    {
        public IMessage Deserialize(byte[] rawMessage)
        {
            return new TestMessage { Text = Encoding.UTF8.GetString(rawMessage) };
        }
    }
}
