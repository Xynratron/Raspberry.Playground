using System;
using Raspberry.IO.Components.Controllers.Pca9685;

namespace Raspberry.Helper
{
    public class ChannelSettings
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.ChannelSettings");

        private readonly IPwmDevice _device;
        public ChannelSettings(IPwmDevice device, PwmChannel channel)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _device = device;
            Channel = channel;
            Step = 5;
            Offset = 0;
            MinPwm = 200;
            MaxPwm = 700;
        }
        public PwmChannel Channel { get; }
        public int CurrentState { get; private set; }
        public int MinPwm { get; set; }
        public int MaxPwm { get; set; }
        public int Offset { get; set; }
        public int Step { get; set; }

        public void Increase()
        {
            var newState = CurrentState + Step;
            if (newState > MaxPwm)
                newState = MaxPwm;
            Log.InfoFormat("Setting Channel {0} to Increase", Channel);
            SetNewState(newState);
        }

        public void Decrease()
        {
            var newState = CurrentState - Step;
            if (newState < MinPwm)
                newState = MinPwm;
            Log.InfoFormat("Setting Channel {0} to Decrease", Channel);
            SetNewState(newState);
        }

        public void Home()
        {
            var newState = (MinPwm + MaxPwm) / 2;
            Log.InfoFormat("Setting Channel {0} to Home", Channel);
            SetNewState(newState);
        }

        private void SetNewState(int newState)
        {
            if (CurrentState != newState)
                CurrentState = newState;
            Log.InfoFormat("Setting Channel {0} to Pwm {1}", Channel, CurrentState);
            _device.SetPwm(Channel, 0, CurrentState);
        }
    }
}