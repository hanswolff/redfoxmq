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
using RedFoxMQ.Transports.InProc;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RedFoxMQ.Tests.Transports.InProc
{
    [TestFixture]
    public class QueueStreamTests
    {
        [Test]
        public void CanRead_is_always_true()
        {
            var stream = new QueueStream();
            Assert.True(stream.CanRead);
        }

        [Test]
        public void CanSeek_is_always_false()
        {
            var stream = new QueueStream();
            Assert.False(stream.CanSeek);
        }

        [Test]
        public void CanWrite_is_always_true()
        {
            var stream = new QueueStream();
            Assert.True(stream.CanWrite);
        }

        [Test]
        public void Length_is_always_0()
        {
            var stream = new QueueStream();
            Assert.AreEqual(0, stream.Length);
        }

        [Test]
        public void SetLength_throws_NotSupportedException()
        {
            var stream = new QueueStream();
            Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
        }

        [Test]
        public void Position_is_always_0()
        {
            var stream = new QueueStream();
            Assert.AreEqual(0, stream.Position);
        }

        [Test]
        public void Flush_is_implemented_but_does_nothing()
        {
            var stream = new QueueStream();
            stream.Flush();
        }

        [Test]
        public void Seek_throws_NotSupportedException()
        {
            var stream = new QueueStream();
            Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        }

        [Test]
        public void Position_setter_throws_NotSupportedException()
        {
            var stream = new QueueStream();
            Assert.Throws<NotSupportedException>(() => stream.Position = 0);
        }

        [Test]
        public void Dispose_stops_blocking_Read_which_throws_ObjectDisposedException()
        {
            var stream = new QueueStream(true);

            Task.Delay(30).ContinueWith(t => stream.Dispose());

            var sw = Stopwatch.StartNew();
            var readBuffer = new byte[1];

            Assert.Throws<ObjectDisposedException>(() => stream.Read(readBuffer, 0, 1));
            Assert.Greater(sw.ElapsedMilliseconds, 15);
        }

        [Test]
        public void Write_after_Dispose_throws_ObjectDisposedException()
        {
            var stream = new QueueStream();
            var buffer = new byte[1];

            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.Write(buffer, 0, 1));
        }

        [Test]
        public void Read_is_blocking_if_enabled_until_read_enough_bytes()
        {
            var stream = new QueueStream(true);

            Task.Delay(30).ContinueWith(t => stream.WriteByte(1));

            var sw = Stopwatch.StartNew();
            var readBuffer = new byte[1];
            stream.Read(readBuffer, 0, 1);
            Assert.Greater(sw.ElapsedMilliseconds, 15);
        }

        [Test]
        public void Read_null_throws_ArgumentNullException()
        {
            var stream = new QueueStream();
            Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 0));
        }

        [Test]
        public void Read_negative_offset_throws_ArgumentOutOfRangeException()
        {
            var stream = new QueueStream();
            var buffer = new byte[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(buffer, -1, 0));
        }

        [Test]
        public void Read_negative_count_throws_ArgumentOutOfRangeException()
        {
            var stream = new QueueStream();
            var buffer = new byte[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(buffer, 0, -1));
        }

        [Test]
        public void Read_is_not_blocking_if_disabled_until_read_enough_bytes()
        {
            var stream = new QueueStream(false);

            Task.Delay(30).ContinueWith(t => stream.WriteByte(1));

            var sw = Stopwatch.StartNew();
            var readBuffer = new byte[1];
            stream.Read(readBuffer, 0, 1);
            Assert.Less(sw.ElapsedMilliseconds, 15);
        }

        [Test]
        public void Read_two_written_single_byte_buffers_in_full()
        {
            var stream = new QueueStream();
            stream.WriteByte(1);
            stream.WriteByte(2);

            var buffer = new byte[2];
            Assert.AreEqual(2, stream.Read(buffer, 0, buffer.Length));

            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
        }

        [Test]
        public void Read_two_written_single_byte_buffers_part()
        {
            var stream = new QueueStream();
            stream.WriteByte(1);
            stream.WriteByte(2);

            var buffer = new byte[1];
            Assert.AreEqual(1, stream.Read(buffer, 0, buffer.Length));

            Assert.AreEqual(1, buffer[0]);
        }

        [Test]
        public void ReadAll_single_multibyte_buffer()
        {
            var stream = new QueueStream();
            var writeBuffer = new byte[] { 1, 2, 3, 4, 5 };
            stream.Write(writeBuffer, 0, writeBuffer.Length);

            var readBuffer = stream.ReadAll();
            Assert.AreEqual(5, readBuffer.Length);
            Assert.AreEqual(1, readBuffer[0]);
            Assert.AreEqual(2, readBuffer[1]);
            Assert.AreEqual(3, readBuffer[2]);
            Assert.AreEqual(4, readBuffer[3]);
            Assert.AreEqual(5, readBuffer[4]);
        }

        [Test]
        public void Read_then_ReadAll_two_written_multibyte_buffers_in_full()
        {
            var stream = new QueueStream();
            var writeBuffer = new byte[] {1, 2, 3, 4, 5};
            stream.Write(writeBuffer, 0, writeBuffer.Length);

            var readBuffer = new byte[2];
            Assert.AreEqual(2, stream.Read(readBuffer, 0, readBuffer.Length));

            Assert.AreEqual(1, readBuffer[0]);
            Assert.AreEqual(2, readBuffer[1]);

            readBuffer = stream.ReadAll();
            Assert.AreEqual(3, readBuffer.Length);
            Assert.AreEqual(3, readBuffer[0]);
            Assert.AreEqual(4, readBuffer[1]);
            Assert.AreEqual(5, readBuffer[2]);
        }

        [Test]
        public void Read_two_written_multibyte_buffers_in_full()
        {
            var stream = new QueueStream();
            var writeBuffer = new byte[] { 1, 2, 3, 4 };
            stream.Write(writeBuffer, 0, 2);
            stream.Write(writeBuffer, 2, 2);

            var readBuffer = new byte[4];
            Assert.AreEqual(4, stream.Read(readBuffer, 0, readBuffer.Length));

            Assert.AreEqual(writeBuffer, readBuffer);
        }

        [Test]
        public void Read_two_written_multibyte_buffers_part()
        {
            var stream = new QueueStream();
            var writeBuffer = new byte[] { 1, 2, 3, 4 };
            stream.Write(writeBuffer, 0, 2);
            stream.Write(writeBuffer, 2, 2);

            var readBuffer = new byte[3];
            Assert.AreEqual(3, stream.Read(readBuffer, 0, readBuffer.Length));

            Assert.AreEqual(writeBuffer[0], readBuffer[0]);
            Assert.AreEqual(writeBuffer[1], readBuffer[1]);
            Assert.AreEqual(writeBuffer[2], readBuffer[2]);

            Assert.AreEqual(1, stream.Read(readBuffer, 0, readBuffer.Length));
            Assert.AreEqual(writeBuffer[3], readBuffer[0]);
        }

        [Test]
        public void Write_null_throws_ArgumentNullException()
        {
            var stream = new QueueStream();
            Assert.Throws<ArgumentNullException>(() => stream.Write(null, 0, 0));
        }

        [Test]
        public void Write_negative_offset_throws_ArgumentOutOfRangeException()
        {
            var stream = new QueueStream();
            var buffer = new byte[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(buffer, -1, 0));
        }

        [Test]
        public void Write_negative_count_throws_ArgumentOutOfRangeException()
        {
            var stream = new QueueStream();
            var buffer = new byte[0];
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(buffer, 0, -1));
        }

        [Test]
        public void Write_middle_of_buffer()
        {
            var stream = new QueueStream();
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            stream.Write(buffer, 1, 3);

            Assert.AreEqual(2, stream.ReadByte());
            Assert.AreEqual(3, stream.ReadByte());
            Assert.AreEqual(4, stream.ReadByte());
        }

        [Test]
        public void Write_multiple_times_Read_once()
        {
            var queueStream = new QueueStream();
            var writeBuffer = new byte[] { 9, 8, 7, 6, 5, 4 };
            queueStream.Write(writeBuffer, 0, 2);
            queueStream.Write(writeBuffer, 2, 2);
            queueStream.Write(writeBuffer, 4, 2);

            var readBuffer = new byte[6];

            queueStream.Read(readBuffer, 0, readBuffer.Length);
            Assert.AreEqual(writeBuffer, readBuffer);
        }

        [Test]
        public void Write_and_Read_random_buffers()
        {
            var stream = new QueueStream();

            var random = TestHelpers.CreateSemiRandomGenerator();
            int totalLength = 0;

            using (var writtenBytes = new MemoryStream())
            using (var readBytes = new MemoryStream())
            {
                for (var i = 0; i < 1000; i++)
                {
                    var buffer = TestHelpers.GetRandomBytes(random, random.Next(10));
                    stream.Write(buffer, 0, buffer.Length);
                    totalLength += buffer.Length;

                    writtenBytes.Write(buffer, 0, buffer.Length);
                }

                while (totalLength > 0)
                {
                    var bytesToRead = Math.Min(random.Next(10), totalLength);
                    var buffer = new byte[bytesToRead];
                    totalLength -= bytesToRead;

                    var read = stream.Read(buffer, 0, buffer.Length);
                    Assert.AreEqual(buffer.Length, read);

                    readBytes.Write(buffer, 0, read);
                }

                Assert.AreEqual(writtenBytes.ToArray(), readBytes.ToArray());
            }
        }

        [Test]
        public void Write_and_ReadAll_random_buffers()
        {
            var stream = new QueueStream();

            var random = TestHelpers.CreateSemiRandomGenerator();

            using (var writtenBytes = new MemoryStream())
            {
                for (int i = 0; i < 1000; i++)
                {
                    var buffer = TestHelpers.GetRandomBytes(random, random.Next(10));
                    stream.Write(buffer, 0, buffer.Length);

                    writtenBytes.Write(buffer, 0, buffer.Length);
                }

                Assert.AreEqual(writtenBytes.ToArray(), stream.ReadAll());
            }
        }
    }
}
