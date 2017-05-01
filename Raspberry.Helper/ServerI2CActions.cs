using System;

namespace Raspberry.Helper
{
    public class ServerI2CActions : ServerI2CActionsBase
    {
        public void AddChannel(EnableI2CChannel message)
        {
            var channelSettings = new ChannelSettings(Device, message.Channel)
            {
                MaxPwm = message.MaxPwm,
                MinPwm = message.MinPwm,
                Offset = message.Offset,
                Step = message.Step
            };
            if (Servos.TryAdd(message.Channel, channelSettings))
            {
                Log.Info($"Added Servo for Channel {message.Channel}.");
                channelSettings.Home();
            }
            else
            {
                Log.Info($"Servo for Channel {message.Channel} could not be added. It may allready exists.");
            }
        }

        public void ServoAction(ServoExecuteMessage message)
        {
            Log.Info($"Got Action {message.Action} for servo {message.Channel}");
            ChannelSettings ChannelSettings;
            if (Servos.TryGetValue(message.Channel, out ChannelSettings))
            {
                switch (message.Action)
                {
                    case I2CChannelAction.Home:
                        ChannelSettings.Home();
                        break;
                    case I2CChannelAction.Increase:
                        ChannelSettings.Increase();
                        break;
                    case I2CChannelAction.Decrease:
                        ChannelSettings.Decrease();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Log.Warn($"Could not Find Servo Settings for Channel {message.Channel}");
            }
        }
    }
}