using RedFoxMQ.Transports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameSender : ISendMessageFrame, IDisconnect
    {
        private static readonly MessageFrameStreamWriter MessageFrameStreamWriter = new MessageFrameStreamWriter();

        private readonly ISocket _socket;
        public MessageFrameSender(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;
        }

        public async Task SendAsync(MessageFrame messageFrame, CancellationToken cancellationToken)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");

            await MessageFrameStreamWriter.WriteMessageFrame(_socket.Stream, messageFrame, cancellationToken);
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }
    }
}
