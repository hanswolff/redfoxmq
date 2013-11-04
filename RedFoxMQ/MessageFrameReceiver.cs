using System;
using System.Threading;
using System.Threading.Tasks;
using RedFoxMQ.Transports;

namespace RedFoxMQ
{
    class MessageFrameReceiver : IReceiveMessageFrame, IDisconnect
    {
        private static readonly MessageFrameStreamReader MessageFrameStreamReader = new MessageFrameStreamReader();

        private readonly ISocket _socket;
        public MessageFrameReceiver(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;
        }

        public async Task<MessageFrame> ReceiveAsync(CancellationToken cancellationToken)
        {
            return await MessageFrameStreamReader.ReadMessageFrame(_socket.Stream, cancellationToken);
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }
    }
}
