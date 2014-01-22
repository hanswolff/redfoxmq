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
    class NodeGreetingMessageNegotiator
    {
        private readonly IStreamSocket _streamSocket;
        private readonly byte[] _singleByteBuffer = new byte[1];

        public NodeGreetingMessageNegotiator(IStreamSocket streamSocket)
        {
            if (streamSocket == null) throw new ArgumentNullException("streamSocket");
            _streamSocket = streamSocket;
        }

        public void WriteGreeting(NodeGreetingMessage greetingMessage)
        {
            var serialized = greetingMessage.Serialize();
            _streamSocket.Write(serialized, 0, serialized.Length);
        }

        public async Task WriteGreetingAsync(NodeGreetingMessage greetingMessage, CancellationToken cancellationToken)
        {
            var serialized = greetingMessage.Serialize();
            await _streamSocket.WriteAsync(serialized, 0, serialized.Length, cancellationToken);
        }

        public void VerifyRemoteGreeting(NodeType expectedNodeType)
        {
            var remoteGreeting = ReadGreeting();
            if (remoteGreeting.NodeType != expectedNodeType)
                throw new RedFoxProtocolException(
                    String.Format("Remote greeting node type was {0} but expected node type is {1}", remoteGreeting.NodeType, expectedNodeType));
        }

        private NodeGreetingMessage ReadGreeting()
        {
            var read = _streamSocket.Read(_singleByteBuffer, 0, 1);
            if (read == 0) throw new RedFoxProtocolException("Error receiving greeting message from remote machine");

            var headerLength = _singleByteBuffer[0];
            var header = new byte[headerLength];

            var offset = 0;
            while (headerLength - offset > 0)
            {
                read = _streamSocket.Read(header, offset, headerLength - offset);
                if (read == 0) throw new RedFoxProtocolException("Error receiving greeting message from remote machine");
                offset += read;
            }

            return NodeGreetingMessage.DeserializeWithoutLength(header);
        }

        public async Task VerifyRemoteGreetingAsync(NodeType expectedNodeType, CancellationToken cancellationToken)
        {
            var remoteGreeting = await ReadGreetingAsync(cancellationToken);
            if (remoteGreeting.NodeType != expectedNodeType)
                throw new RedFoxProtocolException(
                    String.Format("Remote greeting node type was {0} but expected node type is {1}", remoteGreeting.NodeType, expectedNodeType));
        }

        private async Task<NodeGreetingMessage> ReadGreetingAsync(CancellationToken cancellationToken)
        {
            var read = await _streamSocket.ReadAsync(_singleByteBuffer, 0, 1, cancellationToken);
            if (read == 0) throw new RedFoxProtocolException("Error receiving greeting message from remote machine");

            var headerLength = _singleByteBuffer[0];
            var header = new byte[headerLength];

            var offset = 0;
            while (headerLength - offset > 0)
            {
                read = await _streamSocket.ReadAsync(header, offset, headerLength - offset, cancellationToken);
                if (read == 0) throw new RedFoxProtocolException("Error receiving greeting message from remote machine");
                offset += read;
            }

            return NodeGreetingMessage.DeserializeWithoutLength(header);
        }
    }
}
