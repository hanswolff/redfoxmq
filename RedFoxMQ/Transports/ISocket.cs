using System.IO;

namespace RedFoxMQ.Transports
{
    interface ISocket : IDisconnect
    {
        RedFoxEndpoint Endpoint { get; }
        Stream Stream { get; }
    }
}
