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

namespace RedFoxMQ.Transports
{
    public class SocketConfiguration : ISocketConfiguration
    {
        public static readonly SocketConfiguration Default = new SocketConfiguration();

        public TimeSpan ConnectTimeout { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }

        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }

        public SocketConfiguration()
        {
            ConnectTimeout = TimeSpan.FromSeconds(10);
            ReceiveTimeout = TimeSpan.FromMilliseconds(0);
            SendTimeout = TimeSpan.FromSeconds(30);
            ReceiveBufferSize = 65536;
            SendBufferSize = 65536;
        }

        public SocketConfiguration Clone()
        {
            return (SocketConfiguration)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
