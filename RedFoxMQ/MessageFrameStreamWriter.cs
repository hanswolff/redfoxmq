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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageFrameStreamWriter : IMessageFrameWriter
    {
        private static readonly ConcurrentQueue<WeakReference<MemoryStream>> RecycledMemoryStreams = new ConcurrentQueue<WeakReference<MemoryStream>>();

        private readonly IStreamSocket _streamSocket;
        public MessageFrameStreamWriter(IStreamSocket streamSocket)
        {
            if (streamSocket == null) throw new ArgumentNullException("streamSocket");
            _streamSocket = streamSocket;
        }

        public void WriteMessageFrame(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (messageFrame.RawMessage == null) throw new ArgumentException("messageFrame.RawMessage cannot be null");

            CreateBufferWriteSingle(messageFrame);
        }

        public async Task WriteMessageFrameAsync(MessageFrame messageFrame, CancellationToken cancellationToken)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (messageFrame.RawMessage == null) throw new ArgumentException("messageFrame.RawMessage cannot be null");

            await CreateBufferWriteSingleAsync(messageFrame, cancellationToken);
        }

        public void WriteMessageFrames(ICollection<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;

            CreateBufferWriteMany(messageFrames);
        }

        public async Task WriteMessageFramesAsync(ICollection<MessageFrame> messageFrames, CancellationToken cancellationToken)
        {
            if (messageFrames == null) return;

            await CreateBufferWriteManyAsync(messageFrames, cancellationToken);
        }

        private void CreateBufferWriteSingle(MessageFrame messageFrame)
        {
            var sendBufferSize = MessageFrame.HeaderSize + messageFrame.RawMessage.Length;

            WeakReference<MemoryStream> reference;
            var mem = GetOrCreateMemoryStream(sendBufferSize, out reference);

            try
            {
                WriteTypeId(mem, messageFrame.MessageTypeId);
                WriteLength(mem, messageFrame.RawMessage.Length);
                WriteBody(mem, messageFrame.RawMessage);

                var toSend = mem.GetBuffer();
                _streamSocket.Write(toSend, 0, sendBufferSize);
            }
            finally
            {
                mem.SetLength(0);
                RecycledMemoryStreams.Enqueue(reference);
            }
        }

        private async Task CreateBufferWriteSingleAsync(MessageFrame messageFrame,
            CancellationToken cancellationToken)
        {
            var sendBufferSize = MessageFrame.HeaderSize + messageFrame.RawMessage.Length;

            WeakReference<MemoryStream> reference;
            var mem = GetOrCreateMemoryStream(sendBufferSize, out reference);

            try
            {
                WriteTypeId(mem, messageFrame.MessageTypeId);
                WriteLength(mem, messageFrame.RawMessage.Length);
                WriteBody(mem, messageFrame.RawMessage);

                var toSend = mem.GetBuffer();
                await _streamSocket.WriteAsync(toSend, 0, sendBufferSize, cancellationToken);
            }
            finally
            {
                mem.SetLength(0);
                RecycledMemoryStreams.Enqueue(reference);
            }
        }

        private static MemoryStream GetOrCreateMemoryStream(int sendBufferSize, out WeakReference<MemoryStream> reference)
        {
            MemoryStream mem;
            if (!RecycledMemoryStreams.TryDequeue(out reference))
            {
                mem = new MemoryStream(sendBufferSize);
                reference = new WeakReference<MemoryStream>(mem);
            }
            else
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (!reference.TryGetTarget(out mem) || mem == null) // Mono issue workaround: check for mem == null
                {
                    mem = new MemoryStream(sendBufferSize);
                    reference.SetTarget(mem);
                }
            }
            return mem;
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

        private void CreateBufferWriteMany(ICollection<MessageFrame> messageFrames)
        {
            if (messageFrames == null) return;
            var sendBufferSize = messageFrames.Count * MessageFrame.HeaderSize + messageFrames.Sum(m => m.RawMessage.Length);

            WeakReference<MemoryStream> reference;
            var mem = GetOrCreateMemoryStream(sendBufferSize, out reference);

            try
            {
                foreach (var messageFrame in messageFrames)
                {
                    WriteTypeId(mem, messageFrame.MessageTypeId);
                    WriteLength(mem, messageFrame.RawMessage.Length);
                    WriteBody(mem, messageFrame.RawMessage);
                }

                var toSend = mem.GetBuffer();
                _streamSocket.Write(toSend, 0, sendBufferSize);
            }
            finally
            {
                mem.SetLength(0);
                RecycledMemoryStreams.Enqueue(reference);
            }
        }

        private async Task CreateBufferWriteManyAsync(ICollection<MessageFrame> messageFrames,
            CancellationToken cancellationToken)
        {
            if (messageFrames == null) return;
            var sendBufferSize = messageFrames.Count * MessageFrame.HeaderSize + messageFrames.Sum(m => m.RawMessage.Length);

            WeakReference<MemoryStream> reference;
            var mem = GetOrCreateMemoryStream(sendBufferSize, out reference);

            try
            {
                foreach (var messageFrame in messageFrames)
                {
                    WriteTypeId(mem, messageFrame.MessageTypeId);
                    WriteLength(mem, messageFrame.RawMessage.Length);
                    WriteBody(mem, messageFrame.RawMessage);
                }

                var toSend = mem.GetBuffer();
                await _streamSocket.WriteAsync(toSend, 0, sendBufferSize, cancellationToken);
            }
            finally
            {
                mem.SetLength(0);
                RecycledMemoryStreams.Enqueue(reference);
            }
        }
    }
}
