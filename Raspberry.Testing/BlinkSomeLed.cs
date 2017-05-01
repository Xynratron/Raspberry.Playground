using Bmf.Shared.Esb;
using Raspberry.Helper;
using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Testing
{
    internal static class BlinkSomeLed
    {
        internal static void Start(ProcessorPin pin = ProcessorPin.Pin17)
        {
            for (int i = 0; i < 10; i++)
            {
                MessageSender.Send(new GpioSetStatus(pin, true));
                System.Threading.Thread.Sleep(500);
                MessageSender.Send(new GpioSetStatus(pin, false));
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}