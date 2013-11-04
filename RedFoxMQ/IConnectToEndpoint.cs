using RedFoxMQ.Transports;

namespace RedFoxMQ
{
    interface IConnectToEndpoint
    {
        void Connect(RedFoxEndpoint endpoint);
    }
}
