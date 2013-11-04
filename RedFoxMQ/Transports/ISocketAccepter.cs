using System;

namespace RedFoxMQ.Transports
{
    interface ISocketAccepter
    {
        event Action<ISocket> ClientConnected;

        void Bind(RedFoxEndpoint endpoint, Action<ISocket> onClientConnected = null);
        void Unbind(bool waitForExit = true);
    }
}
