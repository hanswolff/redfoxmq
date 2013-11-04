using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.InProc
{
    class InProcessSocketAccepter : ISocketAccepter
    {
        private BlockingCollection<QueueStream> _listener;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public event Action<ISocket> ClientConnected = client => { };

        private RedFoxEndpoint _endpoint;
        public void Bind(RedFoxEndpoint endpoint, Action<ISocket> onClientConnected = null)
        {
            if (_listener != null || !_stopped.IsSet)
                throw new InvalidOperationException("Server already bound, please use Unbind first");

            _listener = InProcessEndpoints.Instance.RegisterAccepter(endpoint);
            _endpoint = endpoint;
            if (onClientConnected != null)
                ClientConnected += onClientConnected;

            _started.Reset();
            _cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => StartAcceptLoop(_cts.Token), TaskCreationOptions.LongRunning);
            _started.Wait();
        }

        private void StartAcceptLoop(CancellationToken cancellationToken)
        {
            _stopped.Reset();
            _started.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var queueStream = _listener.Take(cancellationToken);
                    TryFireClientConnectedEvent(queueStream);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _stopped.Set();
            }
        }

        private bool TryFireClientConnectedEvent(QueueStream queueStream)
        {
            try
            {
                var socket = new InProcSocket(_endpoint, queueStream);
                ClientConnected(socket);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Unbind(bool waitForExit = true)
        {
            var listener = Interlocked.Exchange(ref _listener, null);
            if (listener != null)
            {
                _cts.Cancel(false);

                if (waitForExit) _stopped.Wait();

                InProcessEndpoints.Instance.UnregisterAccepter(_endpoint);
            }
        }
    }
}
