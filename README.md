## RedFoxMQ

RedFoxMQ is a .NET in-memory message queue that uses a simple TCP transport. It is fairly lightweight
and provides full control over message serialization / de-serialization. The performance is very
good (batch broadcasting over TCP reaches >2 million (!) messages per second on my machine).

#### Supported Features

- easy integration (no external components needed)
- implement your own message serialization / deserialization
- Publisher / Subscriber scenario
- Request / Response scenario
- ServiceQueue scenario
- TCP / InProc transport
- message batching

#### Planned Features

- reliable transport (to confirm receipt of messages)
- shared memory transport for faster inter-process communication

Also I'm sure there are still bugs. So do not use it for production yet! (unless you don't care of course)

#### Unsupported Features

- message persistence
- message timestamps
- unique message IDs (e.g. to detect duplicate messages)
- encryption

The features above are not going to be supported to keep the message queue 
lightweight. But there are ways around it. You could implement some these features 
within your specific use case (e.g. just add timestamps to all of your messages, 
if you need timestamps).

#### Usage Example

The easiest way is to look at the unit tests. They are a good source of examples.

Or have a look at a standalone Request / Response example:

```c#
	using RedFoxMQ;
	using RedFoxMQ.Transports;
	using System;
	using System.Text;

	class Program
	{
		static void Main()
		{
			var messageSerialization = new MessageSerialization();
			
			messageSerialization.RegisterSerializer( // register serializer for each message type
				TestMessage.UniqueIdPerMessageType, 
				new TestMessageSerializer());
				
			messageSerialization.RegisterDeserializer( // register deserializer for each message type
				TestMessage.UniqueIdPerMessageType, 
				new TestMessageDeserializer());

			// setup simple echo responder
			Func<IMessage, IMessage> echoFunc = request => request; // alternatively implement IResponderWorker instead
			var echoWorker = new ResponderWorker(echoFunc);
			var workerFactory = new ResponderWorkerFactory(request => echoWorker);

			using (var responder = new Responder(workerFactory, messageSerialization))
			using (var requester = new Requester(messageSerialization))
			{
				var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);
				responder.Bind(endpoint); // call Bind multiple times to listen to multiple endpoints

				requester.Connect(endpoint);

				foreach (var text in new[] {"Hello", "World"})
				{
					var requestMessage = new TestMessage {Text = text};
					var responseMessage = (TestMessage) requester.Request(requestMessage);

					Console.WriteLine(responseMessage.Text);
				}
			}

			Console.ReadLine();
		}

		class TestMessageSerializer : IMessageSerializer
		{
			public byte[] Serialize(IMessage message)
			{
				var testMessage = (TestMessage)message;
				return Encoding.UTF8.GetBytes(testMessage.Text);
			}
		}

		class TestMessageDeserializer : IMessageDeserializer
		{
			public IMessage Deserialize(byte[] rawMessage)
			{
				return new TestMessage { Text = Encoding.UTF8.GetString(rawMessage) };
			}
		}

		class TestMessage : IMessage
		{
			public const ushort UniqueIdPerMessageType = 1;

			public ushort MessageTypeId { get { return UniqueIdPerMessageType; } }
			public string Text { get; set; }
		}    
	}
```

I recommend using [Protocol Buffers](https://code.google.com/p/protobuf-net/)
for message serialization, but it is entirely up to you!

#### Contact

Please let me know if there are bugs or if you have suggestions how to improve the code.
I accept pull requests.

And maybe follow me on Twitter [@quadfinity](https://twitter.com/quadfinity) :)
