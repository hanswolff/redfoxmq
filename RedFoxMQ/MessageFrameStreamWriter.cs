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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameStreamWriter
    {
        public async Task WriteMessageFrame(Stream stream, MessageFrame messageFrame, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (messageFrame.RawMessage == null) throw new ArgumentException("messageFrame.RawMessage cannot be null");

            byte[] toSend;

            using (var mem = new MemoryStream())
            {
                WriteTypeId(mem, messageFrame.MessageTypeId);
                WriteLength(mem, messageFrame.RawMessage.Length);
                WriteBody(mem, messageFrame.RawMessage);

                toSend = mem.ToArray();
            }

            await stream.WriteAsync(toSend, 0, toSend.Length, cancellationToken);
        }

        private static void WriteTypeId(Stream stream, ushort messageTypeId)
        {
            var bytes = BitConverter.GetBytes(messageTypeId);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteLength(Stream stream, int length)
        {
            var bytes = BitConverter.GetBytes(length);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteBody(Stream stream, byte[] rawMessage)
        {
            stream.Write(rawMessage, 0, rawMessage.Length);
        }
    }
}
