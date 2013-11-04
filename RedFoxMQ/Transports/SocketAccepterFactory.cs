using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;
using System;

namespace RedFoxMQ.Transports
{
    class SocketAccepterFactory
    {
        public ISocketAccepter CreateForTransport(RedFoxTransport transport)
        {
            switch (transport)
            {
                case RedFoxTransport.Inproc:
                    return new InProcessSocketAccepter();
                case RedFoxTransport.Tcp:
                    return new TcpSocketAccepter();
                default:
                    throw new NotSupportedException(String.Format("Transport {0} not supported", transport));
            }
        }

        public ISocketAccepter CreateAndBind(RedFoxEndpoint endpoint, Action<ISocket> onClientConnected = null)
        {
            var server = CreateForTransport(endpoint.Transport);
            server.Bind(endpoint, onClientConnected);
            return server;
        }
    }
}
