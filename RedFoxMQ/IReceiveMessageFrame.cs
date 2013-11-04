using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    interface IReceiveMessageFrame
    {
        Task<MessageFrame> ReceiveAsync(CancellationToken cancellationToken);
    }
}
