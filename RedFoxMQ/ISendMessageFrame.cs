using System.Threading;
using System.Threading.Tasks;

namespace RedFoxMQ
{
    interface ISendMessageFrame
    {
        Task SendAsync(MessageFrame messageFrame, CancellationToken cancellationToken);
    }
}
