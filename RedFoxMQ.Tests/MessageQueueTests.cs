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

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageQueueTests
    {
        [Test]
        public void MessageQueue_Add_fires_MessageFramesAdded()
        {
            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueue(4096);
            MessageFrame messageFrame = null;
            messageQueue.MessageFramesAdded += m => { messageFrame = m.Single(); };

            messageQueue.Add(testMessageFrame);
            
            Assert.AreSame(testMessageFrame, messageFrame);
        }

        [Test]
        public void MessageQueue_AddRange_fires_MessageFramesAdded()
        {
            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueue(4096);
            MessageFrame messageFrame = null;
            messageQueue.MessageFramesAdded += m => { messageFrame = m.Single(); };

            messageQueue.AddRange(new [] { testMessageFrame });

            Assert.AreSame(testMessageFrame, messageFrame);
        }

        [Test]
        public void MessageQueue_SendFromQueue_fires_MessageFramesAdded()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessagFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueue(4096);
            messageQueue.Add(testMessageFrame);

            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.AreSame(testMessageFrame, messageFramesWritten.First());
        }

        private static IMessageFrameWriter CreateMessagFrameWriter(List<MessageFrame> messageFramesWritten)
        {
            var mock = new Mock<IMessageFrameWriter>(MockBehavior.Strict);

            mock.Setup(x => x.WriteMessageFrame(It.IsAny<MessageFrame>()))
                .Callback<MessageFrame>(messageFramesWritten.Add);

            mock.Setup(x => x.WriteMessageFrames(It.IsAny<ICollection<MessageFrame>>()))
                .Callback<ICollection<MessageFrame>>(messageFramesWritten.AddRange);

            return mock.Object;
        }
    }
}
