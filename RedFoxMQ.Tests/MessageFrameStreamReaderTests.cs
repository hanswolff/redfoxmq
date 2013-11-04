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
using System.Threading;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageFrameStreamReaderTests
    {
        [Test]
        public void MessageFrameStreamReader_reads_MessageTypeId_then_Length_then_RawMessage()
        {
            var rawMessage = new byte[] {1, 2, 3, 4, 5};
            var serializedMessageFrame = new byte[] {49, 212, 5, 0, 0, 0, 1, 2, 3, 4, 5};

            using (var mem = new MemoryStream(serializedMessageFrame))
            {
                var reader = new MessageFrameStreamReader();
                var messageFrame = reader.ReadMessageFrame(mem, CancellationToken.None).Result;

                Assert.AreEqual(54321, messageFrame.MessageTypeId);
                Assert.AreEqual(rawMessage, messageFrame.RawMessage);
                Assert.LessOrEqual(messageFrame.TimestampReceived, DateTime.UtcNow);
            }
        }
    }
}
