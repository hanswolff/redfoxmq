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
using System.Diagnostics;
using System.Threading;

namespace RedFoxMQ
{
    public class ResponderWorkerScheduler : IDisposable
    {
        private int _minThreads;
        private readonly int _maxThreads;

        private readonly BlockingCollection<ResponderWorkerWithState> _workers = new BlockingCollection<ResponderWorkerWithState>();
        private readonly ConcurrentDictionary<Guid, Thread> _threads = new ConcurrentDictionary<Guid, Thread>();
        private readonly object _threadsChangeLock = new object();

        private int _currentWorkerThreadCount;
        public int CurrentWorkerThreadCount
        {
            get { return _currentWorkerThreadCount; }
        }

        private int _currentBusyThreadCount;
        public int CurrentBusyThreadCount
        {
            get { return _currentBusyThreadCount; }
        }

        public event Action<IResponderWorker, object, IMessage> WorkerCompleted = (wu, s, m) => { };
        public event Action<IResponderWorker, object, Exception> WorkerException = (wu, s, e) => { };

        private TimeSpan _maxIdleTime;
        /// <summary>
        /// Maximum idle time of a worker thread before shutting down
        /// (only if current number of threads is higher than MinThreads)
        /// </summary>
        public TimeSpan MaxIdleTime
        {
            get { return _maxIdleTime; }
            set { _maxIdleTime = value; }
        }

        public ResponderWorkerScheduler()
            : this(0, 1, TimeSpan.FromMinutes(1))
        {
        }

        public ResponderWorkerScheduler(int minThreads, int maxThreads)
            : this(minThreads, maxThreads, TimeSpan.FromMinutes(1))
        {
        }

        public ResponderWorkerScheduler(int minThreads, int maxThreads, TimeSpan maxIdleTime)
        {
            if (minThreads < 0)
                throw new ArgumentOutOfRangeException("minThreads", minThreads, String.Format("minThreads cannot be negative (but was {0})", minThreads));
            if (maxThreads <= 0)
                throw new ArgumentOutOfRangeException("maxThreads", maxThreads, String.Format("maxThreads must be larger than zero (but was {0})", maxThreads));
            if (maxThreads < minThreads)
                throw new ArgumentOutOfRangeException("maxThreads", maxThreads, String.Format("maxThreads cannot be smaller than minThreads (minThreads was {0}, maxThreads was {1})", minThreads, maxThreads));

            _minThreads = minThreads;
            _maxThreads = maxThreads;
            MaxIdleTime = maxIdleTime;

            _disposedToken = _disposedTokenSource.Token;

            CreateNumberOfThreads(minThreads);
        }

        private void CreateNumberOfThreads(int numberOfTasks)
        {
            for (var i = 0; i < numberOfTasks; i++)
            {
                var threadId = Guid.NewGuid();
                var thread = new Thread(ExecuteTask) { IsBackground = true };

                _threads[threadId] = thread;
                thread.Start(threadId);

                Interlocked.Increment(ref _currentWorkerThreadCount);
            }
        }

        private void ExecuteTask(object argument)
        {
            var threadId = (Guid)argument;

            try
            {
                do
                {
                    ResponderWorkerWithState workerWithState;
                    if (!_workers.TryTake(out workerWithState, (int) MaxIdleTime.TotalMilliseconds, _disposedToken))
                    {
                        if (ShutdownTaskIfNotNeeded(threadId)) break;
                        continue;
                    }

                    Interlocked.Increment(ref _currentBusyThreadCount);

                    try
                    {
                        IMessage response = null;
                        try
                        {
                            response = workerWithState.Worker.GetResponse(workerWithState.RequestMessage,
                                workerWithState.State);
                        }
                        catch (Exception ex)
                        {
                            WorkerException(workerWithState.Worker, workerWithState.State, ex);
                        }

                        if (response != null)
                        {
                            WorkerCompleted(workerWithState.Worker, workerWithState.State, response);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore exception from fired events
                        Debug.WriteLine(ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _currentBusyThreadCount);
                    }

                } while (true);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                ShutdownTaskIfNotNeeded(threadId);
            }
        }

        private bool ShutdownTaskIfNotNeeded(Guid threadId)
        {
            if (_currentWorkerThreadCount <= _minThreads && !_disposed) return false;

            lock (_threadsChangeLock)
            {
                if (_currentWorkerThreadCount <= _minThreads && !_disposed) return false;
                
                Thread thread;
                if (_threads.TryRemove(threadId, out thread))
                {
                    Interlocked.Decrement(ref _currentWorkerThreadCount);
                }
            }

            return true;
        }

        public void AddWorker(IResponderWorker worker, IMessage requestMessage, object state)
        {
            if (worker == null) throw new ArgumentNullException("worker");
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);

            _workers.TryAdd(new ResponderWorkerWithState(worker, requestMessage, state));

            IncreaseWorkerThreadsIfNeeded();
        }

        private void IncreaseWorkerThreadsIfNeeded()
        {
            if (!CanIncreaseWorkerThreads()) return;

            lock (_threadsChangeLock)
            {
                if (!CanIncreaseWorkerThreads()) return;

                CreateNumberOfThreads(1);
            }
        }

        private bool CanIncreaseWorkerThreads()
        {
            var workerThreadCount = _currentWorkerThreadCount;
            if (workerThreadCount != 0 &&
                (_currentBusyThreadCount != workerThreadCount || workerThreadCount >= _maxThreads)) return false;
            return true;
        }

        #region Dispose
        private bool _disposed;
        private readonly CancellationTokenSource _disposedTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _disposedToken;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    _minThreads = 0;

                    try { _disposedTokenSource.Cancel(); }
                    catch (AggregateException) { }

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ResponderWorkerScheduler()
        {
            Dispose(false);
        }
        #endregion

    }
}
