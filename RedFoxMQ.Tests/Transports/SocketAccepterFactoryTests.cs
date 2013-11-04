using NUnit.Framework;
using RedFoxMQ.Transports;
using RedFoxMQ.Transports.InProc;
using RedFoxMQ.Transports.Tcp;

namespace RedFoxMQ.Tests.Transports
{
    [TestFixture]
    public class SocketAccepterFactoryTests
    {
        [Test]
        public void InProcessTransport_creates_InProcessBroadcastSocketAccepter()
        {
            var accepter = new SocketAccepterFactory().CreateForTransport(RedFoxTransport.Inproc);
            Assert.IsInstanceOf<InProcessSocketAccepter>(accepter);
        }

        [Test]
        public void TcpTransport_creates_TcpClientBroadcastSocketAccepter()
        {
            var accepter = new SocketAccepterFactory().CreateForTransport(RedFoxTransport.Tcp);
            Assert.IsInstanceOf<TcpSocketAccepter>(accepter);
        }
    }
}
