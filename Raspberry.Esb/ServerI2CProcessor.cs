using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;
using Raspberry.Helper;

namespace Raspberry.Esb
{
    public class ServerI2CProcessor : ServerI2CActions, IReceiver<EnableI2CChannel>,
        IReceiver<ServoExecuteMessage>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, EnableI2CChannel message)
        {
            AddChannel(message);
        }

        public void ReceiveMessage(IEnvironment environment, Envelope envelope, ServoExecuteMessage message)
        {
            ServoAction(message);
        }
    }
}