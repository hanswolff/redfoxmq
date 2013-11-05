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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.InProc
{
    class QueueStream : Stream
    {
        readonly BlockingCollection<byte[]> _buffers = new BlockingCollection<byte[]>();
        byte[] _currentBuffer;
        int _currentBufferOffset;

        private readonly bool _blocking;
        private int _length;

        public override long Length
        {
            get { return _length; }
        }

        public QueueStream()
        {
        }

        public QueueStream(bool blocking)
        {
            _blocking = blocking;
        }

        private readonly CancellationTokenSource _disposeCancellationTokenSource = new CancellationTokenSource();
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _disposeCancellationTokenSource.Cancel(false);
        }

        public byte[] ReadAll()
        {
            var bytesAlreadyRead = 0;

            var localBuffers = new Queue<byte[]>();
            byte[] localBuffer;
            byte[] firstBuffer = null;
            int localBuffersSize = 0;
            while (_buffers.TryTake(out localBuffer))
            {
                if (firstBuffer == null) firstBuffer = localBuffer;
                localBuffersSize += localBuffer.Length;
                localBuffers.Enqueue(localBuffer);
            }

            if (firstBuffer != null &&
                localBuffersSize == firstBuffer.Length &&
                _currentBuffer == null)
            {
                // in many scenarios this is sufficient and prevents unnecessary 
                // buffer creation and memory copy operations
                return firstBuffer;
            }

            var bufferSize = _currentBuffer != null ? _currentBuffer.Length - _currentBufferOffset : 0;
            bufferSize += localBuffersSize;

            var buffer = new byte[bufferSize];

            do
            {
                if (_currentBuffer == null || _currentBufferOffset == _currentBuffer.Length)
                {
                    if (localBuffers.Count == 0)
                        return buffer;
                    _currentBuffer = localBuffers.Dequeue();
                    _currentBufferOffset = 0;
                }

                var bytesAvailable = _currentBuffer.Length - _currentBufferOffset;
                var bytesToReadFromCurrentBuffer = bytesAvailable;

                Array.Copy(_currentBuffer, _currentBufferOffset, buffer, bytesAlreadyRead, bytesToReadFromCurrentBuffer);

                _currentBufferOffset += bytesToReadFromCurrentBuffer;
                bytesAlreadyRead += bytesToReadFromCurrentBuffer;
                Interlocked.Add(ref _length, -bytesToReadFromCurrentBuffer);

            } while (true);
        }

        public int Read(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", String.Format("Offset cannot be negative (but was: {0})", offset));
            if (count == 0) return 0;
            if (count < 0) throw new ArgumentOutOfRangeException("count", String.Format("Cannot read a negative number of bytes (parameter 'count' is: {0})", count));

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_disposeCancellationTokenSource.Token, cancellationToken))
            {
                var bytesAlreadyRead = 0;
                do
                {
                    if (_currentBuffer == null || _currentBufferOffset == _currentBuffer.Length)
                    {
                        if (_blocking)
                        {
                            try
                            {
                                _currentBuffer = _buffers.Take(tokenSource.Token);
                            }
                            catch (OperationCanceledException)
                            {
                                if (_disposeCancellationTokenSource.IsCancellationRequested)
                                    throw new ObjectDisposedException(typeof(QueueStream).Name);
                                throw;
                            }
                        }
                        else
                        {
                            if (tokenSource.IsCancellationRequested || !_buffers.TryTake(out _currentBuffer))
                                return bytesAlreadyRead;
                        }
                        _currentBufferOffset = 0;
                    }

                    var bytesToRead = count - bytesAlreadyRead;
                    var bytesAvailable = _currentBuffer.Length - _currentBufferOffset;

                    var bytesToReadFromCurrentBuffer = Math.Min(bytesToRead, bytesAvailable);
                    Array.Copy(_currentBuffer, _currentBufferOffset, buffer, offset + bytesAlreadyRead,
                        bytesToReadFromCurrentBuffer);

                    _currentBufferOffset += bytesToReadFromCurrentBuffer;
                    bytesAlreadyRead += bytesToReadFromCurrentBuffer;
                    Interlocked.Add(ref _length, -bytesToRead);

                } while (bytesAlreadyRead < count);

                return bytesAlreadyRead;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", String.Format("Offset cannot be negative (but was: {0})", offset));
            if (count == 0) return;
            if (count < 0) throw new ArgumentOutOfRangeException("count", String.Format("Cannot write a negative number of bytes (parameter 'count' is: {0})", count));
            if (_disposeCancellationTokenSource.IsCancellationRequested)
                throw new ObjectDisposedException(typeof(QueueStream).Name);

            var splice = new byte[count];
            Array.Copy(buffer, offset, splice, 0, count);
            _buffers.Add(splice);
            Interlocked.Add(ref _length, count);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Stream is not seekable");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot set stream length");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer, offset, count, CancellationToken.None);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Run(() => Read(buffer, offset, count, cancellationToken), cancellationToken);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Position
        {
            get { return 0; }
            set { throw new NotSupportedException("Stream is not seekable"); }
        }
    }
}
