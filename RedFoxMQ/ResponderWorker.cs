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
    public class ResponderWorker : IResponderWorker
    {
        private static readonly Func<IMessage, IMessage> EchoFunction = request => request;

        private readonly Func<IMessage, IMessage> _responderFunc;

        public ResponderWorker()
        {
            _responderFunc = EchoFunction;
        }

        public ResponderWorker(Func<IMessage, IMessage> responderFunc)
        {
            if (responderFunc == null) throw new ArgumentNullException("responderFunc");
            _responderFunc = responderFunc;
        }

        public IMessage GetResponse(IMessage requestMessage, object state)
        {
            return _responderFunc(requestMessage);
        }
    }

    public class ResponderWorker<T> : IResponderWorker, IResponderWorker<T> where T : IMessage
    {
        private static readonly Func<T, IMessage> EchoFunction = request => request;

        private readonly Func<T, IMessage> _responderFunc;

        public ResponderWorker()
        {
            _responderFunc = EchoFunction;
        }

        public ResponderWorker(Func<T, IMessage> responderFunc)
        {
            if (responderFunc == null) throw new ArgumentNullException("responderFunc");
            _responderFunc = responderFunc;
        }

        public IMessage GetResponse(T requestMessage, object state)
        {
            return _responderFunc(requestMessage);
        }

        public IMessage GetResponse(IMessage requestMessage, object state)
        {
            return _responderFunc((T)requestMessage);
        }
    }
}
