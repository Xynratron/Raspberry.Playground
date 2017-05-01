using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;

namespace Esb.Raspberry
{
    public class ServerI2CActionsAddChannel : ServerI2CActionsBase, IReceiver<EnableI2CChannel>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, EnableI2CChannel message)
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
    }
}