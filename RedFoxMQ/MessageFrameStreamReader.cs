using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameStreamReader
    {
        public async Task<MessageFrame> ReadMessageFrame(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var header = new byte[6];
            var offset = 0;
            while (offset < header.Length)
            {
                var read = await stream.ReadAsync(header, 0, header.Length, cancellationToken);
                offset += read;
            }

            var messageTypeId = BitConverter.ToUInt16(header, 0);
            var length = BitConverter.ToInt32(header, 2);
            var rawMessage = await ReadBody(stream, length, cancellationToken);

            return new MessageFrame
            {
                MessageTypeId = messageTypeId,
                RawMessage = rawMessage,
                TimestampReceived = DateTime.UtcNow
            };
        }

        private static async Task<byte[]> ReadBody(Stream stream, int length, CancellationToken cancellationToken)
        {
            var rawMessage = new byte[length];

            var offset = 0;
            while (offset < length)
            {
                var read = await stream.ReadAsync(rawMessage, offset, rawMessage.Length, cancellationToken);
                offset += read;
            }
            return rawMessage;
        }
    }
}
