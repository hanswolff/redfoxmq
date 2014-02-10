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

namespace RedFoxMQ
{
    /// <summary>
    /// Responder factory that maps a responder by message type
    /// </summary>
    public class TypeMappedResponderWorkerFactory : IResponderWorkerFactory
    {
        private readonly Func<IMessage, IResponderWorker>[] _map;
        
        private Func<IMessage, IResponderWorker> _defaultFunc;
        /// <summary>
        /// Default message handler, if message type wasn't mapped 
        /// (default behavior is response message := request message)
        /// </summary>
        public Func<IMessage, IResponderWorker> DefaultFunc
        {
            get { return _defaultFunc; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _defaultFunc = value;
            }
        }

        public TypeMappedResponderWorkerFactory()
        {
            _map = new Func<IMessage, IResponderWorker>[ushort.MaxValue];

            CreateDefaultMap();
        }

        private void CreateDefaultMap()
        {
            Func<IMessage, IMessage> echoFunc = m => m;
            var echoResponderWorker = new ResponderWorker(echoFunc);
            DefaultFunc = m => echoResponderWorker;

            for (var i = 0; i < _map.Length; i++)
            {
                _map[i] = m => _defaultFunc(m);
            }
        }

        public void Map<T>(IResponderWorker responderWorker) where T : IMessage, new()
        {
            var messageTypeId = new T().MessageTypeId;
            _map[messageTypeId] = m => responderWorker;
        }

        public void Map<T>(IResponderWorker<T> responderWorker) where T : IMessage, new()
        {
            var messageTypeId = new T().MessageTypeId;
            _map[messageTypeId] = m => responderWorker;
        }

        public void Map<T>(Func<T, IMessage> responseFunc) where T : IMessage, new()
        {
            var messageTypeId = new T().MessageTypeId;
            _map[messageTypeId] = m => new ResponderWorker<T>(responseFunc);
        }

        public IResponderWorker GetWorkerFor(IMessage requestMessage)
        {
            if (requestMessage == null) throw new ArgumentNullException("requestMessage");
            var func = _map[requestMessage.MessageTypeId];
            return func(requestMessage);
        }
    }
}
