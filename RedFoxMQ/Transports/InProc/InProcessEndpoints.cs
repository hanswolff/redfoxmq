// 
// Copyright 2013 Hans Wolff
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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
            _registeredAccepterPorts = new ConcurrentDictionary<RedFoxEndpoint, BlockingCollection<InProcSocketPair>>();
        }

        private readonly ConcurrentDictionary<RedFoxEndpoint, BlockingCollection<InProcSocketPair>> _registeredAccepterPorts;

        public BlockingCollection<InProcSocketPair> RegisterAccepter(RedFoxEndpoint endpoint)
        {
            if (endpoint.Transport != RedFoxTransport.Inproc)
                throw new ArgumentException("Only InProcess transport endpoints are allowed to be registered");

            var result = _registeredAccepterPorts.AddOrUpdate(
                endpoint,
                e => new BlockingCollection<InProcSocketPair>(),
                (e, v) =>
                {
                    throw new InvalidOperationException("Endpoint already listening to InProcess clients");
                });
            return result;
        }

        public InProcSocket Connect(RedFoxEndpoint endpoint)
        {
            if (endpoint.Transport != RedFoxTransport.Inproc)
                throw new ArgumentException("Only InProcess transport endpoints are allowed to be registered");

            BlockingCollection<InProcSocketPair> accepter;
            if (!_registeredAccepterPorts.TryGetValue(endpoint, out accepter))
            {
                throw new InvalidOperationException("Endpoint not listening to InProcess clients");
            }

            var clientStream = new QueueStream(true);
            var serverStream = new QueueStream(true);
            var clientSocket = new InProcSocket(endpoint, clientStream, serverStream);
            var serverSocket = new InProcSocket(endpoint, serverStream, clientStream);

            var socketPair = new InProcSocketPair(clientSocket, serverSocket);
            accepter.Add(socketPair);

            return clientSocket;
        }

        public bool UnregisterAccepter(RedFoxEndpoint endpoint)
        {
            BlockingCollection<InProcSocketPair> oldValue;
            return _registeredAccepterPorts.TryRemove(endpoint, out oldValue);
        }
    }
}
