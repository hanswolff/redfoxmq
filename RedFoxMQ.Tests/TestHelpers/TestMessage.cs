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

// ReSharper disable once CheckNamespace
namespace RedFoxMQ.Tests
{
    class TestMessage : IMessage
    {
        public const int TypeId = 1;
        public ushort MessageTypeId { get { return TypeId; } }

        public string Text { get; set; }

        public TestMessage(string text = "")
        {
            Text = text;
        }

        public override bool Equals(object obj)
        {
            var testMessage = obj as TestMessage;
            if (testMessage == null) return false;
            return Text == testMessage.Text;
        }

        protected bool Equals(TestMessage other)
        {
            return string.Equals(Text, other.Text);
        }

        public override int GetHashCode()
        {
            return (Text != null ? Text.GetHashCode() : 0);
        }
    }
}
