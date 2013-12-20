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

        public event Action<ISocket, ISocketConfiguration> ClientConnected = (socket, socketConfig) => { };
        public event Action<ISocket> ClientDisconnected = client => { };

        public void Bind(RedFoxEndpoint endpoint, ISocketConfiguration socketConfiguration, Action<ISocket, ISocketConfiguration> onClientConnected = null, Action<ISocket> onClientDisconnected = null)
        {
            if (_listener != null || !_stopped.IsSet)
                throw new InvalidOperationException("Server already bound, please use Unbind first");

            var ipAddress = IpAddressFromHostTranslator.GetIpAddressForHost(endpoint.Host);

            _endpoint = endpoint;
            _listener = new TcpListener(ipAddress, endpoint.Port);

            if (onClientConnected != null)
                ClientConnected += onClientConnected;
            if (onClientDisconnected != null)
                ClientDisconnected += onClientDisconnected;

            _stopped.Reset();
            _started.Reset();

            _listener.Start();

            _cts = new CancellationTokenSource();

            StartAcceptLoop(socketConfiguration, _cts.Token);
        }

        private void StartAcceptLoop(ISocketConfiguration socketConfiguration, CancellationToken cancellationToken)
        {
            var task = AcceptLoopAsync(socketConfiguration, cancellationToken);
        }

        private async Task AcceptLoopAsync(ISocketConfiguration socketConfiguration, CancellationToken cancellationToken)
        {
            _started.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    SetupTcpClientParameters(tcpClient, socketConfiguration);

                    var socket = new TcpSocket(_endpoint, tcpClient);
                    socket.Disconnected += () => ClientDisconnected(socket);

                    TryFireClientConnectedEvent(socket, socketConfiguration);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                _stopped.Set();
            }
        }

        private static void SetupTcpClientParameters(TcpClient tcpClient, ISocketConfiguration socketConfiguration)
        {
            tcpClient.ReceiveTimeout = socketConfiguration.ReceiveTimeout.ToMillisOrZero();
            tcpClient.SendTimeout = socketConfiguration.SendTimeout.ToMillisOrZero();

            tcpClient.NoDelay = true;
            tcpClient.ReceiveBufferSize = socketConfiguration.ReceiveBufferSize;
            tcpClient.SendBufferSize = socketConfiguration.SendBufferSize;
        }

        private bool TryFireClientConnectedEvent(ISocket socket, ISocketConfiguration socketConfiguration)
        {
            try
            {
                ClientConnected(socket, socketConfiguration);
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
            if (listener == null) return;

            _cts.Cancel(false);
            listener.Stop();

            if (waitForExit) _stopped.Wait();
        }
    }
}
