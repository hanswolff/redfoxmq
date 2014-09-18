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

using System;
using ProtoBuf;

namespace RedFoxMQ.Serialization.ProtoBuf.Tests
{
    [ProtoContract]
    class TestMessage : IMessage
    {
        [ProtoIgnore]
        public ushort MessageTypeId { get { return 1; } }

        [ProtoMember(1)]
        public string String { get; set; }

        [ProtoMember(2)]
        public bool Boolean { get; set; }

        [ProtoMember(3)]
        public int Int32 { get; set; }

        [ProtoMember(4)]
        public long Int64 { get; set; }

        [ProtoMember(5)]
        public Guid Guid { get; set; }
    }
}
