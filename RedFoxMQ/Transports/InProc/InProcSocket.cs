using System;
using System.IO;

namespace RedFoxMQ.Transports.InProc
{
    class InProcSocket : ISocket
    {
        public RedFoxEndpoint Endpoint { get; private set; }
        public Stream Stream { get; private set; }

        public InProcSocket(RedFoxEndpoint endpoint, QueueStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            Endpoint = endpoint;
            Stream = stream;
        }

        public void Disconnect()
        {
        }
    }
}
