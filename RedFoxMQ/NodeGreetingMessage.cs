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

namespace RedFoxMQ
{
    class NodeGreetingMessage
    {
        private const byte ProtocolVersion = 1;

        public NodeType NodeType { get; private set; }

        public NodeGreetingMessage(NodeType nodeType)
        {
            NodeType = nodeType;
        }

        public static NodeGreetingMessage Deserialize(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length < 2) 
                throw new RedFoxProtocolException(String.Format("Protocol header (excluding size) must have at least 2 bytes payload but had only {0} bytes", buffer.Length));

            if (buffer[0] != ProtocolVersion)
                throw new RedFoxProtocolException(String.Format("Protocol version {0} is not supported (expected {1})", buffer[0], ProtocolVersion));

            var message = new NodeGreetingMessage((NodeType)buffer[1]);
            return message;
        }

        public byte[] Serialize()
        {
            var result = new byte[3];
            result[0] = (byte)(result.Length - 1);
            result[1] = ProtocolVersion;
            result[2] = (byte) NodeType;
            return result;
        }
    }
}
