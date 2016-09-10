using Raspberry.IO.Components.Controllers.Pca9685;

namespace Esb.Raspberry
{
    public class ServoExecuteMessage
    {
        public I2CChannelAction Action { get; set; }
        public PwmChannel Channel { get; set; }
    }
}