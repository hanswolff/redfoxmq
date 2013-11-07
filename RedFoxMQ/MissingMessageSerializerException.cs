﻿// 
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

namespace RedFoxMQ
{
    public class MissingMessageSerializerException : MessageSerializationException
    {
        public MissingMessageSerializerException()
            : base(String.Format("No message serializer defined for message"))
        {
        }

        public MissingMessageSerializerException(int messageTypeId)
            : base(String.Format("No message serializer defined for message type ID {0}", messageTypeId))
        {
        }

        public MissingMessageSerializerException(int messageTypeId, Type messageType)
            : base(String.Format("No message serializer defined for message type ID {0} (message type: {1})", messageTypeId, messageType))
        {
        }
    }
}
