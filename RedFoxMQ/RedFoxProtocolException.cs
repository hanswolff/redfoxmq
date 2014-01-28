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
    /// Exception that is thrown when the protocol or expected node type
    /// of a connecting node does not match
    /// </summary>
    public class RedFoxProtocolException : RedFoxBaseException
    {
        private const string DefaultMessage = "RedFoxMQ Message Protocol Error";

        public RedFoxProtocolException()
            : base(DefaultMessage)
        {
        }

        public RedFoxProtocolException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public RedFoxProtocolException(string message)
            : base(message)
        {
        }

        public RedFoxProtocolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
