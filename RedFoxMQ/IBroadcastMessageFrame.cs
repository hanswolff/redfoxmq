using System.Threading.Tasks;

namespace RedFoxMQ
{
    interface IBroadcastMessageFrame
    {
        Task BroadcastAsync(MessageFrame messageFrame);
    }
}
