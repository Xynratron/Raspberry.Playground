using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Helper
{
    public class GpioPinServer
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.GpioPinServer");
        protected static GpioConnection GpioConnection = new GpioConnection();

        public void SetStatus(GpioSetStatus message)
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