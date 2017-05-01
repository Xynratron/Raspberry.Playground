using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;
using Raspberry.Helper;

namespace Raspberry.Esb
{
    public class GpioPinServerProcessor : GpioPinServer, IReceiver<GpioSetStatus>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, GpioSetStatus message)
        {
            SetStatus(message);
        }
    }
}