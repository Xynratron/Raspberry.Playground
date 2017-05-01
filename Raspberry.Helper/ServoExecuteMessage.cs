using Raspberry.IO.Components.Controllers.Pca9685;

namespace Raspberry.Helper
{
    public class ServoExecuteMessage
    {
        public I2CChannelAction Action { get; set; }
        public PwmChannel Channel { get; set; }
    }
}