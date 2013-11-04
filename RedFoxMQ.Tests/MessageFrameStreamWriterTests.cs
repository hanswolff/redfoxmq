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
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageFrameStreamWriterTests
    {
        [Test]
        public void MessageFrameStreamWriter_writes_MessageTypeId_then_Length_then_RawMessage()
        {
            var messageFrame = new MessageFrame
            {
                MessageTypeId = 54321,
                RawMessage = new byte[] { 1, 2, 3, 4, 5}
            };

            using (var mem = new MemoryStream())
            {
                var writer = new MessageFrameStreamWriter();
                writer.WriteMessageFrame(mem, messageFrame, CancellationToken.None).Wait();

                var writtenToStream = mem.ToArray();
                
                Assert.AreEqual(messageFrame.MessageTypeId, BitConverter.ToUInt16(writtenToStream, 0));
                Assert.AreEqual(messageFrame.RawMessage.Length, BitConverter.ToUInt16(writtenToStream, 2));

                Assert.AreEqual(messageFrame.RawMessage, writtenToStream.Skip(6).ToArray());
            }
        }
    }
}
