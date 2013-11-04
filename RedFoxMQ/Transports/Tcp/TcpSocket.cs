using System;
using System.IO;
using System.Net.Sockets;

namespace RedFoxMQ.Transports.Tcp
{
    public class TcpSocket : ISocket
    {
        private readonly TcpClient _tcpClient;

        private readonly Stream _stream;
        public Stream Stream
        {
            get { return _stream; }
        }

        public RedFoxEndpoint Endpoint { get; private set; }

        public TcpSocket(RedFoxEndpoint endpoint, TcpClient tcpClient)
        {
            if (tcpClient == null) throw new ArgumentNullException("tcpClient");
            _tcpClient = tcpClient;

            Endpoint = endpoint;
            _stream = tcpClient.GetStream();
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }
    }
}
