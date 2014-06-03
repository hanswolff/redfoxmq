// 
// Copyright 2013-2014 Hans Wolff
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

using System.IO;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.Tcp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class NodeGreetingMessageVerifier
    {
        private static readonly NodeGreetingMessageNegotiatorFactory NodeGreetingMessageNegotiatorFactory = new NodeGreetingMessageNegotiatorFactory();
        private readonly NodeGreetingMessage _greetingMessage;
        private readonly HashSet<NodeType> _expectedRemoteNodeTypes;

        public NodeGreetingMessageVerifier(NodeType localNodeType, NodeType expectedRemoteNodeType, params NodeType[] allowOtherRemoteNoteTypes)
        {
            _expectedRemoteNodeTypes = new HashSet<NodeType> { expectedRemoteNodeType };
            if (allowOtherRemoteNoteTypes != null) _expectedRemoteNodeTypes.UnionWith(allowOtherRemoteNoteTypes);

            _greetingMessage = new NodeGreetingMessage(localNodeType);
        }

        public async Task<NodeType> SendReceiveAndVerifyAsync(ISocket socket, TimeSpan timeout)
        {
            var greetingMessageNegotiator = NodeGreetingMessageNegotiatorFactory.CreateFromSocket(socket);

            var cancellationTokenSource = new CancellationTokenSource(timeout.ToMillisOrZero());
            var token = cancellationTokenSource.Token;
            greetingMessageNegotiator.WriteGreetingAsync(_greetingMessage, token).ConfigureAwait(false);

            var taskReadGreeting = await greetingMessageNegotiator.VerifyRemoteGreetingAsync(_expectedRemoteNodeTypes, token).ConfigureAwait(false);
            return taskReadGreeting.NodeType;
        }

        public NodeType SendReceiveAndVerify(ISocket socket, TimeSpan timeout)
        {
            var greetingMessageNegotiator = NodeGreetingMessageNegotiatorFactory.CreateFromSocket(socket);

            var tcpSocket = socket as TcpSocket;
            var sendReceiveTimeout = new Tuple<int, int>(0, 0);
            if (tcpSocket != null)
            {
                sendReceiveTimeout = new Tuple<int, int>(tcpSocket.TcpClient.SendTimeout,
                    tcpSocket.TcpClient.ReceiveTimeout);

                tcpSocket.TcpClient.SendTimeout = timeout.ToMillisOrZero();
                tcpSocket.TcpClient.ReceiveTimeout = timeout.ToMillisOrZero();
            }

            try
            {
                greetingMessageNegotiator.WriteGreeting(_greetingMessage);

                var readGreeting = greetingMessageNegotiator.VerifyRemoteGreeting(_expectedRemoteNodeTypes);
                return readGreeting.NodeType;
            }
            catch (IOException)
            {
                throw new TimeoutException("Timeout occurred negotiating after connection had been established");
            }
            finally
            {
                if (tcpSocket != null)
                {
                    tcpSocket.TcpClient.SendTimeout = sendReceiveTimeout.Item1;
                    tcpSocket.TcpClient.ReceiveTimeout = sendReceiveTimeout.Item2;
                }
            }
        }
    }
}
