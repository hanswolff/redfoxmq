
using System.Text;

namespace RedFoxMQ.Tests.TestHelpers
{
    class TestMessageSerializer : IMessageSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            var testMessage = (TestMessage) message;
            return Encoding.UTF8.GetBytes(testMessage.Text);
        }
    }
}
