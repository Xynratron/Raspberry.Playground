using Raspberry.IO.Components.Controllers.Pca9685;

namespace Esb.Raspberry
{
    public class EnableI2CChannel
    {
        public EnableI2CChannel(PwmChannel channel, int minPwm = 200, int maxPwm = 700, int offset = 0, int step = 5)
        {
            Step = step;
            Offset = offset;
            Channel = channel;
            MinPwm = minPwm;
            MaxPwm = maxPwm;
        }

        public PwmChannel Channel { get; }
        public int MinPwm { get;  }
        public int MaxPwm { get; }
        public int Offset { get; }
        public int Step { get; }
    }
}