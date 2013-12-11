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
using RedFoxMQ.Transports;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    interface IConnectToEndpoint
    {
        void Connect(RedFoxEndpoint endpoint);
        void Connect(RedFoxEndpoint endpoint, int timeoutInSeconds);

        Task ConnectAsync(RedFoxEndpoint endpoint);
        Task ConnectAsync(RedFoxEndpoint endpoint, int timeoutInSeconds);
    }
}
