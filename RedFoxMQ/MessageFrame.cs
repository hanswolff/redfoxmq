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
using System.Linq;
using System.Text;

namespace RedFoxMQ
{
    class MessageFrame : IEquatable<MessageFrame>
    {
        private const int SizeMessageTypeId = sizeof(ushort);
        private const int SizeLength = sizeof(int);

        public const int HeaderSize = SizeMessageTypeId + SizeLength;

        public ushort MessageTypeId;
        public byte[] RawMessage;

        public bool Equals(MessageFrame other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return MessageTypeId == other.MessageTypeId &&
                   RawMessage.LongLength == other.RawMessage.LongLength &&
                   RawMessage.SequenceEqual(other.RawMessage);
        }

        public override string ToString()
        {
            return String.Format(
                "MessageFrame: {{ MessageTypeId: {0}, RawMessage[{1}]: {2} }}",
                MessageTypeId,
                RawMessage != null ? RawMessage.Length : 0,
                GetFirstHexBytesOfRawMessage(5));
        }

        private string GetFirstHexBytesOfRawMessage(int numberOfBytes)
        {
            var rawMessage = RawMessage;
            if (rawMessage == null) return "(null)";

            var hexBytes = rawMessage.Take(numberOfBytes).Select(x => x.ToString("x2"));
            var result = new StringBuilder(String.Join(" ", hexBytes));
            if (rawMessage.Length > numberOfBytes) result.Append("...");

            return result.ToString();
        }
    }
}
