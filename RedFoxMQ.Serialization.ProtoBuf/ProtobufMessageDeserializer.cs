// 
// Copyright 2014 Hans Wolff
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

using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RedFoxMQ.Serialization.ProtoBuf
{
    public class ProtobufMessageDeserializer : IMessageDeserializer
    {
        private readonly Func<Stream, IMessage>[] _deserializerMap = new Func<Stream, IMessage>[UInt16.MaxValue];

        public void AssignAssemblyMessageTypes(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new [] { Assembly.GetEntryAssembly() };

            var messageInterface = typeof (IMessage);
            var funcType = typeof(Func<,>).MakeGenericType(typeof(Stream), typeof(IMessage));

            var deserializeMethod = typeof (Serializer)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.IsGenericMethod)
                .Where(x => x.Name == "Deserialize")
                .Where(x => x.GetParameters().Count() == 1)
                .Single(x => x.GetParameters()[0].ParameterType == typeof (Stream));

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(messageInterface.IsAssignableFrom);

                foreach (var messageType in types)
                {
                    var constructor = messageType.GetConstructor(Type.EmptyTypes);
                    if (constructor == null) continue;

                    var message = (IMessage) constructor.Invoke(null);
                    var messageTypeId = message.MessageTypeId;

                    var genericMethod = deserializeMethod.MakeGenericMethod(messageType);
                    var deserializerFunc = (Func<Stream, IMessage>) Delegate.CreateDelegate(funcType, genericMethod);

                    SetDeserializer(messageTypeId, deserializerFunc);
                }
            }
        }

        public Func<Stream, IMessage> GetDeserializer(ushort messageTypeId)
        {
            return _deserializerMap[messageTypeId];
        }

        public void SetDeserializer(ushort messageTypeId, Func<Stream, IMessage> deserializerFunc)
        {
            _deserializerMap[messageTypeId] = deserializerFunc;
        }

        public IMessage Deserialize(byte[] rawMessage)
        {
            if (rawMessage == null) throw new ArgumentNullException("rawMessage");

            using (var mem = new MemoryStream(rawMessage))
            using (var reader = new BinaryReader(mem))
            {
                var messageTypeId = reader.ReadUInt16();
                var deserializer = _deserializerMap[messageTypeId];
                if (deserializer == null)
                {
                    throw new MessageDeserializationException("Missing deserializer for message with type ID " + messageTypeId);
                }
                return deserializer(mem);
            }
        }
    }
}
