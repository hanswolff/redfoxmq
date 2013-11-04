using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.Tcp
{
    class TcpSocketAccepter : ISocketAccepter
    {
        private static readonly IpAddressFromHostTranslator IpAddressFromHostTranslator = new IpAddressFromHostTranslator();

        private RedFoxEndpoint _endpoint;
        private TcpListener _listener;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public event Action<ISocket> ClientConnected = client => { };

        public void Bind(RedFoxEndpoint endpoint, Action<ISocket> onClientConnected = null)
        {
            if (_listener != null || !_stopped.IsSet) 
                throw new InvalidOperationException("Server already bound, please use Unbind first");

            var ipAddress = IpAddressFromHostTranslator.GetIpAddressForHost(endpoint.Host);

            _endpoint = endpoint;
            _listener = new TcpListener(ipAddress, endpoint.Port);
            if (onClientConnected != null)
                ClientConnected += onClientConnected;

            _listener.Start();

            _cts = new CancellationTokenSource();

            _started.Reset();
            Task.Factory.StartNew(() => StartAcceptLoop(_cts.Token), TaskCreationOptions.LongRunning);
            _started.Wait();
        }

        private void StartAcceptLoop(CancellationToken cancellationToken)
        {
            var task = AcceptLoopAsync(cancellationToken);
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            _stopped.Reset();
            _started.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    TryFireClientConnectedEvent(tcpClient);
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

        private bool TryFireClientConnectedEvent(TcpClient tcpClient)
        {
            try
            {
                var socket = new TcpSocket(_endpoint, tcpClient);
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
                listener.Stop();

                if (waitForExit) _stopped.Wait();
            }
        }
    }
}
