using System;
using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;

namespace Esb.Raspberry
{
    public class ServerI2CActions : ServerI2CActionsBase, IReceiver<ServoExecuteMessage>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, ServoExecuteMessage message)
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