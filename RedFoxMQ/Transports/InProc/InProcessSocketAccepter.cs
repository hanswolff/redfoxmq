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
using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ.Transports.InProc
{
    class InProcessSocketAccepter : ISocketAccepter
    {
        private BlockingCollection<InProcSocketPair> _listener;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ManualResetEventSlim _started = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);

        public event Action<ISocket> ClientConnected = client => { };
        public event Action<ISocket> ClientDisconnected = client => { };

        private RedFoxEndpoint _endpoint;
        public void Bind(RedFoxEndpoint endpoint, SocketMode socketMode, Action<ISocket> onClientConnected = null, Action<ISocket> onClientDisconnected = null)
        {
            if (_listener != null || !_stopped.IsSet)
                throw new InvalidOperationException("Server already bound, please use Unbind first");

            _listener = InProcessEndpoints.Instance.RegisterAccepter(endpoint);
            _endpoint = endpoint;
            if (onClientConnected != null)
                ClientConnected += onClientConnected;
            if (onClientDisconnected != null)
                ClientDisconnected += onClientDisconnected;

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
                    var socketPair = _listener.Take(cancellationToken);
                    TryFireClientConnectedEvent(socketPair.ServerSocket);
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

        private bool TryFireClientConnectedEvent(InProcSocket socket)
        {
            try
            {
                socket.Disconnected += () => ClientDisconnected(socket);
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
