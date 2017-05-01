using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;
using Raspberry.Helper;
using Raspberry.IO.GeneralPurpose;

namespace Esb.Raspberry
{
    public class GpioPinServerProcessor : GpioPinServer, IReceiver<GpioSetStatus>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, GpioSetStatus message)
        {
            SetStatus(message);
        }
    }
}