using Bmf.Shared.Esb.Types;

namespace Bmf.Shared.Esb
{
    public interface IReceiverAndSender<in T, R> : IReceiverAndSender
    {
        R ReceiveAndSendMessage(IEnvironment environment, Envelope envelope, T message);
    }

    public interface IReceiverAndSender
    {
    }
}