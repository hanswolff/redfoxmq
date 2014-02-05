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

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedFoxMQ.Tests
{
    [TestFixture]
    public class MessageQueueSingleTests
    {
        [Test]
        public void MessageQueue_empty_MessageCounterSignal_IsSet_false()
        {
            var messageQueue = new MessageQueueSingle();
            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_MessageCounterSignal_IsSet_true()
        {
            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            Assert.IsTrue(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_fires_MessageFrameAdded()
        {
            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            MessageFrame messageFrame = null;
            messageQueue.MessageFrameAdded += m => { messageFrame = m; };

            messageQueue.Add(testMessageFrame);
            
            Assert.AreSame(testMessageFrame, messageFrame);
        }

        [Test]
        public void MessageQueue_AddRange_fires_MessageFrameAdded()
        {
            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            MessageFrame messageFrame = null;
            messageQueue.MessageFrameAdded += m => { messageFrame = m; };

            messageQueue.AddRange(new [] { testMessageFrame });

            Assert.AreSame(testMessageFrame, messageFrame);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueue_message_writer_receives_message()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.AreSame(testMessageFrame, messageFramesWritten.First());
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueue_MessageCounterSignal_IsSet_false()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueue_fails_MessageCounterSignal_IsSet_true()
        {
            var errorMessageFrameWriter = CreateMessageFrameWriterThrowsIOException();

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            Assert.Throws<IOException>(() => messageQueue.SendFromQueue(errorMessageFrameWriter));

            Assert.IsTrue(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueue_fails_once_then_succeeds_MessageCounterSignal_IsSet_false()
        {
            var errorMessageFrameWriter = CreateMessageFrameWriterThrowsIOException();
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            Assert.Throws<IOException>(() => messageQueue.SendFromQueue(errorMessageFrameWriter));
            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_SendFromQueue_writer_receive_first_message()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame1 = new MessageFrame();
            var testMessageFrame2 = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame1, testMessageFrame2 });

            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.AreSame(testMessageFrame1, messageFramesWritten.Single());
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_SendFromQueue_twice_writer_receives_both_messages()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame1 = new MessageFrame();
            var testMessageFrame2 = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame1, testMessageFrame2 });

            messageQueue.SendFromQueue(messageFramesWriter);
            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.AreSame(testMessageFrame1, messageFramesWritten.First());
            Assert.AreSame(testMessageFrame2, messageFramesWritten.Skip(1).Single());
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_after_SendFromQueue_MessageCounterSignal_IsSet_true()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame, testMessageFrame });

            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.IsTrue(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_after_SendFromQueue_twice_MessageCounterSignal_IsSet_false()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame, testMessageFrame });

            messageQueue.SendFromQueue(messageFramesWriter);
            messageQueue.SendFromQueue(messageFramesWriter);

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueueAsync_message_writer_receives_message()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.AreSame(testMessageFrame, messageFramesWritten.First());
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueueAsync_MessageCounterSignal_IsSet_false()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueueAsync_fails_MessageCounterSignal_IsSet_true()
        {
            var errorMessageFrameWriter = CreateMessageFrameWriterThrowsIOException();

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            Assert.Throws<AggregateException>(() => messageQueue.SendFromQueueAsync(errorMessageFrameWriter, CancellationToken.None).Wait());

            Assert.IsTrue(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_Add_single_message_SendFromQueueAsync_fails_once_then_succeeds_MessageCounterSignal_IsSet_false()
        {
            var errorMessageFrameWriter = CreateMessageFrameWriterThrowsIOException();
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.Add(testMessageFrame);

            Assert.Throws<AggregateException>(() => messageQueue.SendFromQueueAsync(errorMessageFrameWriter, CancellationToken.None).Wait());
            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_SendFromQueueAsync_writer_receive_first_message()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame1 = new MessageFrame();
            var testMessageFrame2 = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame1, testMessageFrame2 });

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.AreSame(testMessageFrame1, messageFramesWritten.Single());
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_SendFromQueueAsync_twice_writer_receives_both_messages()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame1 = new MessageFrame();
            var testMessageFrame2 = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame1, testMessageFrame2 });

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();
            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.AreSame(testMessageFrame1, messageFramesWritten.First());
            Assert.AreSame(testMessageFrame2, messageFramesWritten.Skip(1).Single());
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_after_SendFromQueueAsync_MessageCounterSignal_IsSet_true()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame, testMessageFrame });

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.IsTrue(messageQueue.MessageCounterSignal.IsSet);
        }

        [Test]
        public void MessageQueue_AddRange_two_messages_after_SendFromQueueAsync_twice_MessageCounterSignal_IsSet_false()
        {
            var messageFramesWritten = new List<MessageFrame>();
            var messageFramesWriter = CreateMessageFrameWriter(messageFramesWritten);

            var testMessageFrame = new MessageFrame();

            var messageQueue = new MessageQueueSingle();
            messageQueue.AddRange(new[] { testMessageFrame, testMessageFrame });

            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();
            messageQueue.SendFromQueueAsync(messageFramesWriter, CancellationToken.None).Wait();

            Assert.IsFalse(messageQueue.MessageCounterSignal.IsSet);
        }

        private static IMessageFrameWriter CreateMessageFrameWriter(List<MessageFrame> messageFramesWritten)
        {
            var mock = new Mock<IMessageFrameWriter>(MockBehavior.Strict);

            mock.Setup(x => x.WriteMessageFrame(It.IsAny<MessageFrame>()))
                .Callback<MessageFrame>(messageFramesWritten.Add);
            mock.Setup(x => x.WriteMessageFrameAsync(It.IsAny<MessageFrame>(), It.IsAny<CancellationToken>()))
                .Callback<MessageFrame, CancellationToken>((m, c) => messageFramesWritten.Add(m))
                .Returns(() => Task.Run(() => { }));

            mock.Setup(x => x.WriteMessageFrames(It.IsAny<ICollection<MessageFrame>>()))
                .Callback<ICollection<MessageFrame>>(messageFramesWritten.AddRange);
            mock.Setup(x => x.WriteMessageFramesAsync(It.IsAny<ICollection<MessageFrame>>(), It.IsAny<CancellationToken>()))
                .Callback<ICollection<MessageFrame>, CancellationToken>((m, c) => messageFramesWritten.AddRange(m))
                .Returns(() => Task.Run(() => { }));

            return mock.Object;
        }

        private static IMessageFrameWriter CreateMessageFrameWriterThrowsIOException()
        {
            var mock = new Mock<IMessageFrameWriter>(MockBehavior.Strict);

            mock.Setup(x => x.WriteMessageFrame(It.IsAny<MessageFrame>()))
                .Callback<MessageFrame>(x => { throw new IOException(); });
            mock.Setup(x => x.WriteMessageFrameAsync(It.IsAny<MessageFrame>(), It.IsAny<CancellationToken>()))
                .Callback<MessageFrame, CancellationToken>((m, c) => { throw new IOException(); })
                .Returns(() => Task.Run(() => { }));

            mock.Setup(x => x.WriteMessageFrames(It.IsAny<ICollection<MessageFrame>>()))
                .Callback<ICollection<MessageFrame>>(x => { throw new IOException(); });
            mock.Setup(x => x.WriteMessageFramesAsync(It.IsAny<ICollection<MessageFrame>>(), It.IsAny<CancellationToken>()))
                .Callback<ICollection<MessageFrame>, CancellationToken>((m, c) => { throw new IOException(); })
                .Returns(() => Task.Run(() => { }));

            return mock.Object;
        }
    }
}
