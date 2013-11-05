using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;
using System;
using System.Net.Sockets;
using System.Threading;

namespace RedFoxMQ.Transports
{
    class SocketFactory
    {
        public ISocket CreateAndConnect(RedFoxEndpoint endpoint)
        {
            switch (endpoint.Transport)
            {
                case RedFoxTransport.Inproc:
                    return CreateInProcSocket(endpoint);
                case RedFoxTransport.Tcp:
                    return CreateTcpSocket(endpoint);
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", endpoint.Transport));
            }
        }

        private static ISocket CreateInProcSocket(RedFoxEndpoint endpoint)
        {
            var queueStream = InProcessEndpoints.Instance.Connect(endpoint);
            Thread.Sleep(10); // TODO: why do we need this?
            return new InProcSocket(endpoint, queueStream);
        }

        private static ISocket CreateTcpSocket(RedFoxEndpoint endpoint)
        {
            var tcpClient = new TcpClient { ReceiveBufferSize = 65536, SendBufferSize = 65536};
            tcpClient.Connect(endpoint.Host, endpoint.Port);
            Thread.Sleep(10); // TODO: why do we need this?

            return new TcpSocket(endpoint, tcpClient);
        }
    }
}
