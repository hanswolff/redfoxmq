using System;
using System.Collections.Concurrent;

namespace RedFoxMQ.Transports.InProc
{
    class InProcessEndpoints
    {
        private static volatile InProcessEndpoints _instance;
        public static InProcessEndpoints Instance
        {
            get
            {
                if (_instance != null) return _instance;
                    return _instance = new InProcessEndpoints();
            }
        }

        private InProcessEndpoints()
        {
            _registeredAccepterPorts = new ConcurrentDictionary<RedFoxEndpoint, BlockingCollection<QueueStream>>();
        }

        private readonly ConcurrentDictionary<RedFoxEndpoint, BlockingCollection<QueueStream>> _registeredAccepterPorts;

        public BlockingCollection<QueueStream> RegisterAccepter(RedFoxEndpoint endpoint)
        {
            if (endpoint.Transport != RedFoxTransport.Inproc)
                throw new ArgumentException("Only InProcess transport endpoints are allowed to be registered");

            var result = _registeredAccepterPorts.AddOrUpdate(
                endpoint,
                e => new BlockingCollection<QueueStream>(),
                (e, v) =>
                {
                    throw new InvalidOperationException("Endpoint already listening to InProcess clients");
                });
            return result;
        }

        public QueueStream Connect(RedFoxEndpoint endpoint)
        {
            if (endpoint.Transport != RedFoxTransport.Inproc)
                throw new ArgumentException("Only InProcess transport endpoints are allowed to be registered");

            BlockingCollection<QueueStream> accepter;
            if (!_registeredAccepterPorts.TryGetValue(endpoint, out accepter))
            {
                throw new InvalidOperationException("Endpoint not listening to InProcess clients");
            }

            var queueStream = new QueueStream(true);
            accepter.Add(queueStream);
            return queueStream;
        }

        public bool UnregisterAccepter(RedFoxEndpoint endpoint)
        {
            BlockingCollection<QueueStream> oldValue;
            return _registeredAccepterPorts.TryRemove(endpoint, out oldValue);
        }
    }
}
