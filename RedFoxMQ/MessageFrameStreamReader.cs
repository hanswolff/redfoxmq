﻿// 
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameStreamReader : IMessageFrameReader
    {
        private readonly IStreamSocket _streamSocket;

        public MessageFrameStreamReader(IStreamSocket streamSocket)
        {
            if (streamSocket == null) throw new ArgumentNullException("streamSocket");
            _streamSocket = streamSocket;
        }

        public async Task<MessageFrame> ReadMessageFrameAsync(CancellationToken cancellationToken)
        {

            var header = await ReadHeaderAsync(_streamSocket, cancellationToken);

            var messageTypeId = BitConverter.ToUInt16(header, 0);
            var length = BitConverter.ToInt32(header, 2);

            var rawMessage = await ReadBodyAsync(_streamSocket, length, cancellationToken);

            return new MessageFrame
            {
                MessageTypeId = messageTypeId,
                RawMessage = rawMessage,
            };
        }

        private static async Task<byte[]> ReadHeaderAsync(IStreamSocket socket, CancellationToken cancellationToken)
        {
            var header = new byte[6];
            var offset = 0;
            while (offset < header.Length)
            {
                var read = await socket.ReadAsync(header, 0, header.Length, cancellationToken);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
            return header;
        }

        private static async Task<byte[]> ReadBodyAsync(IStreamSocket socket, int length, CancellationToken cancellationToken)
        {
            var rawMessage = new byte[length];

            var offset = 0;
            while (offset < length)
            {
                var read = await socket.ReadAsync(rawMessage, offset, rawMessage.Length - offset, cancellationToken);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
            return rawMessage;
        }

        public MessageFrame ReadMessageFrame()
        {
            var header = ReadHeader(_streamSocket);

            var messageTypeId = BitConverter.ToUInt16(header, 0);
            var length = BitConverter.ToInt32(header, 2);

            var rawMessage = ReadBody(_streamSocket, length);

            return new MessageFrame
            {
                MessageTypeId = messageTypeId,
                RawMessage = rawMessage,
            };
        }

        private static byte[] ReadHeader(IStreamSocket stream)
        {
            var header = new byte[6];
            var offset = 0;
            while (offset < header.Length)
            {
                var read = stream.Read(header, offset, header.Length - offset);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
            return header;
        }

        private static byte[] ReadBody(IStreamSocket stream, int length)
        {
            var rawMessage = new byte[length];

            var offset = 0;
            while (offset < length)
            {
                var read = stream.Read(rawMessage, offset, rawMessage.Length - offset);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
            return rawMessage;
        }
    }
}
