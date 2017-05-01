using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;
using Raspberry.IO.GeneralPurpose;

namespace Esb.Raspberry
{
    public class GpioPinServerSetStatus : GpioPinServer, IReceiver<GpioSetStatus>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, GpioSetStatus message)
        {
            lock (GpioConnection)
            {
                if (!GpioConnection.Contains(message.Pin))
                {
                    Log.Info($"Adding Pin {message.Pin}.");
                    GpioConnection.Add(message.Pin.Output());
                }
                Log.Info($"Setting Status of Pin {message.Pin} to {message.State}.");
                GpioConnection[message.Pin] = message.State;
            }
        }
    }
}