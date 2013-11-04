namespace RedFoxMQ.Tests.TestHelpers
{
    class TestMessage : IMessage
    {
        public ushort MessageTypeId { get; private set; }
        public string Text { get; set; }

        public TestMessage()
        {
            MessageTypeId = 1;
        }
    }
}
