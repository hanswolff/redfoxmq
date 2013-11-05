# RedFoxMQ

RedFoxMQ is a .NET in-memory message queue that uses a simple TCP transport. It is fairly low-level and
provides full control over message serialization / de-serialization.

## Supported Features

- Publisher / Subscriber scenario
- Request / Response scenario
- TCP transport or InProc transport

## Planned Features

- message batching

## Usage Example

The easiest way is to look at the unit tests. They are a good source of examples, e.g.:

    [TestFixture]
    public class RequestResponderTests
    {
        [Test]
        public void Request_Response_single_message()
        {
            using (var responder = new Responder())
            using (var requester = new Requester())
            {
                var endpoint = new RedFoxEndpoint(RedFoxTransport.Tcp, "localhost", 5555, null);

                responder.Bind(endpoint);
                requester.ConnectAsync(endpoint).Wait();

                var messageSent = new TestMessage { Text = "Hello" };
                var messageReceived = (TestMessage)requester.Request(messageSent).Result;

                Assert.AreEqual(messageSent.Text, messageReceived.Text);
            }
        }

        [SetUp]
        public void Setup()
        {
            MessageSerialization.Instance.RegisterSerializer(new TestMessage().MessageTypeId, new TestMessageSerializer());
            MessageSerialization.Instance.RegisterDeserializer(new TestMessage().MessageTypeId, new TestMessageDeserializer());
        }
    }

## Contact

Please let me know if there are bugs or if you have suggestions how to improve the code.
I accept pull requests.

And maybe follow me [@quadfinity](https://twitter.com/quadfinity) :)
