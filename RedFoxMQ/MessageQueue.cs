using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    class MessageQueue : IDisposable
    {
        private readonly MessageQueueProcessor _messageQueueProcessor;
        private readonly MessageFrameSender _sender;
        private readonly BlockingCollection<MessageFrame> _messageFrames = new BlockingCollection<MessageFrame>();

        public event Action<MessageFrame> MessageFrameAdded = m => { };

        public MessageQueue(MessageQueueProcessor messageQueueProcessor, MessageFrameSender sender)
        {
            if (messageQueueProcessor == null) throw new ArgumentNullException("messageQueueProcessor");
            if (sender == null) throw new ArgumentNullException("sender");
            _sender = sender;

            _messageQueueProcessor = messageQueueProcessor;
            messageQueueProcessor.Register(this);
        }

        public void Add(MessageFrame messageFrame)
        {
            if (messageFrame == null) throw new ArgumentNullException("messageFrame");
            if (_messageFrames.TryAdd(messageFrame))
            {
                MessageFrameAdded(messageFrame);
            }
        }

        internal async Task<bool> SendFromQueue(CancellationToken cancellationToken)
        {
            MessageFrame messageFrame;
            if (!_messageFrames.TryTake(out messageFrame)) return false;

            await _sender.SendAsync(messageFrame, cancellationToken);
            return true;
        }

        #region Dispose
        private bool _disposed;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    _messageQueueProcessor.Unregister(this);

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~MessageQueue()
        {
            Dispose(false);
        }
        #endregion

    }
}
