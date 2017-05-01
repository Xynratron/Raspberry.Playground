using Raspberry.IO.GeneralPurpose;

namespace Esb.Raspberry
{
    public class GpioPinServer
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.GpioPinServer");
        protected static GpioConnection GpioConnection = new GpioConnection();
    }
}