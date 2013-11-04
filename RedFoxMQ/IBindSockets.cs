using RedFoxMQ.Transports;

namespace RedFoxMQ
{
    interface IBindSockets
    {
        void Bind(RedFoxEndpoint endpoint);
    }
}
