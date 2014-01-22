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

using RedFoxMQ.Transports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class NodeGreetingMessageVerifier
    {
        private static readonly NodeGreetingMessageNegotiatorFactory NodeGreetingMessageNegotiatorFactory = new NodeGreetingMessageNegotiatorFactory();
        private readonly NodeGreetingMessage _greetingMessage;
        private readonly NodeType _expectedRemoteNodeType;

        public NodeGreetingMessageVerifier(NodeType localNodeType, NodeType expectedRemoteNodeType)
        {
            _expectedRemoteNodeType = expectedRemoteNodeType;
            _greetingMessage = new NodeGreetingMessage(localNodeType);
        }

        public async Task SendReceiveAndVerify(ISocket socket, TimeSpan timeout)
        {
            var greetingMessageNegotiator = NodeGreetingMessageNegotiatorFactory.CreateFromSocket(socket);

            var cancellationTokenSource = new CancellationTokenSource(timeout.ToMillisOrZero());
            var taskWriteGreeting = greetingMessageNegotiator.WriteGreetingAsync(_greetingMessage, cancellationTokenSource.Token);

            var taskReadGreeting = greetingMessageNegotiator.VerifyRemoteGreetingAsync(_expectedRemoteNodeType, cancellationTokenSource.Token);
            await Task.WhenAll(taskWriteGreeting, taskReadGreeting);
        }
    }
}
