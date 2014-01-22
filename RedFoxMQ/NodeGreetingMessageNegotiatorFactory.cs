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

using RedFoxMQ.Transports;
using System;

namespace RedFoxMQ
{
    class NodeGreetingMessageNegotiatorFactory
    {
        public INodeGreetingMessageNegotiator CreateFromSocket(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            var streamSocket = socket as IStreamSocket;
            if (streamSocket != null)
                return new NodeGreetingMessageStreamSocketNegotiator(streamSocket);

            var queueSocket = socket as IQueueSocket;
            if (queueSocket != null)
                return new NodeGreetingMessageQueueSocketNegotiator(queueSocket);

            throw new NotSupportedException(String.Format("{0} does not know ISocket of type {1}", GetType().Name, socket.GetType().Name));
        }
    }
}
