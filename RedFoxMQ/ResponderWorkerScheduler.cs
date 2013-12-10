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

namespace RedFoxMQ
{
    public class ResponderWorkerScheduler : IDisposable
    {
        private readonly int _minThreads;
        private readonly int _maxThreads;

        private readonly BlockingCollection<ResponderWorkUnitWithState> _workUnits = new BlockingCollection<ResponderWorkUnitWithState>();
        private readonly ConcurrentDictionary<Guid, Thread> _threads = new ConcurrentDictionary<Guid, Thread>();

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

        public event Action<IResponderWorkUnit, object, IMessage> WorkUnitCompleted = (wu, s, m) => { };
        public event Action<IResponderWorkUnit, object, Exception> WorkUnitException = (wu, s, e) => { };

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
            var taskId = (Guid)argument;
            var cancellationToken = _disposedTokenSource.Token;

            try
            {
                do
                {
                    ResponderWorkUnitWithState workUnitWithState;
                    if (!TryGetWorkUnit(out workUnitWithState, cancellationToken)) continue;

                    Interlocked.Increment(ref _currentBusyThreadCount);
                    try
                    {
                        IMessage response = null;
                        try
                        {
                            response = workUnitWithState.WorkUnit.GetResponse(workUnitWithState.State);
                        }
                        catch (Exception ex)
                        {
                            WorkUnitException(workUnitWithState.WorkUnit, workUnitWithState.State, ex);
                        }

                        if (response != null)
                        {
                            WorkUnitCompleted(workUnitWithState.WorkUnit, workUnitWithState.State, response);
                        }
                    }
                    catch
                    {
                        // TODO: error handling
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _currentBusyThreadCount);
                    }
                } while (!ShutdownTaskIfNotNeeded(taskId));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private bool TryGetWorkUnit(out ResponderWorkUnitWithState workUnitWithState, CancellationToken cancellationToken)
        {
            return _workUnits.TryTake(out workUnitWithState, (int)MaxIdleTime.TotalMilliseconds, cancellationToken);
        }

        private bool ShutdownTaskIfNotNeeded(Guid threadId)
        {
            if (_currentWorkerThreadCount <= _minThreads) return false;

            // TODO: race condition CurrentWorkerThreadCount could return value below MinThreads for a brief moment of time
            var workerThreadCount = Interlocked.Decrement(ref _currentWorkerThreadCount);
            if (workerThreadCount < _minThreads)
            {
                Interlocked.Increment(ref _currentWorkerThreadCount);
                return false;
            }

            Thread thread;
            _threads.TryRemove(threadId, out thread);
            return true;
        }

        public void AddWorkUnit(IResponderWorkUnit workUnit, object state)
        {
            if (workUnit == null) throw new ArgumentNullException("workUnit");
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);

            _workUnits.TryAdd(new ResponderWorkUnitWithState(workUnit, state));

            var workerThreadCount = _currentWorkerThreadCount;
            if (workerThreadCount == 0 || (_currentBusyThreadCount == workerThreadCount && workerThreadCount < _maxThreads))
            {
                // TODO: increase number of worker threads
            }
        }

        #region Dispose
        private bool _disposed;
        private readonly CancellationTokenSource _disposedTokenSource = new CancellationTokenSource();
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
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
