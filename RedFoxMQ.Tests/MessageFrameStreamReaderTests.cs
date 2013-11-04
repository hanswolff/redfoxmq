using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageFrameStreamReaderTests
    {
        [Test]
        public void MessageFrameStreamReader_reads_MessageTypeId_then_Length_then_RawMessage()
        {
            var rawMessage = new byte[] {1, 2, 3, 4, 5};
            var serializedMessageFrame = new byte[] {49, 212, 5, 0, 0, 0, 1, 2, 3, 4, 5};

            using (var mem = new MemoryStream(serializedMessageFrame))
            {
                var reader = new MessageFrameStreamReader();
                var messageFrame = reader.ReadMessageFrame(mem, CancellationToken.None).Result;

                Assert.AreEqual(54321, messageFrame.MessageTypeId);
                Assert.AreEqual(rawMessage, messageFrame.RawMessage);
                Assert.LessOrEqual(messageFrame.TimestampReceived, DateTime.UtcNow);
            }
        }
    }
}
