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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

// ReSharper disable once CheckNamespace
namespace RedFoxMQ.Tests
{
    public static class TestHelpers
    {
        public static readonly RedFoxEndpoint TcpTestEndpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", GetFreePort(), null);

        public static RedFoxEndpoint CreateEndpointForTransport(RedFoxTransport transport)
        {
            return new RedFoxEndpoint(transport, "localhost", GetFreePort(), null);
        }

        public static RedFoxEndpoint CreateEndpointForTransport(RedFoxTransport transport, int port)
        {
            return new RedFoxEndpoint(transport, "localhost", port, null);
        }

        public static int GetFreePort()
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            var usedPorts = new HashSet<int>(tcpConnInfoArray.Select(x => x.LocalEndPoint.Port));
            var availablePorts = new HashSet<int>(Enumerable.Range(1024, 65535 - 1024));
            availablePorts.ExceptWith(usedPorts);

            return availablePorts.Skip(new Random().Next(availablePorts.Count - 1)).First();
        }

        public static Random CreateSemiRandomGenerator()
        {
            var now = DateTime.Now;
            return new Random(now.DayOfYear * 365 + now.Hour);
        }

        public static byte[] GetRandomBytes(Random random, int size)
        {
            var result = new byte[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = (byte)random.Next(256);
            }
            return result;
        }

        public static void InitializeMessageSerialization()
        {
            MessageSerialization.Instance.RegisterSerializer(TestMessage.TypeId, new TestMessageSerializer());
            MessageSerialization.Instance.RegisterDeserializer(TestMessage.TypeId, new TestMessageDeserializer());

            MessageSerialization.Instance.RegisterSerializer(ExceptionTestMessage.TypeId, new ExceptionTestMessageSerializer());
            MessageSerialization.Instance.RegisterDeserializer(ExceptionTestMessage.TypeId, new ExceptionTestMessageDeserializer());
        }
    }
}
