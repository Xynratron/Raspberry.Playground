using System.Security.Cryptography.X509Certificates;
using Bmf.Shared.Esb.Types;

namespace Bmf.Shared.Esb
{
    public interface IReceiver<in T> : IReceiver
    {
        void ReceiveMessage(IEnvironment environment, Envelope envelope, T message);
    }

    public interface IReceiver
    {
    }
}