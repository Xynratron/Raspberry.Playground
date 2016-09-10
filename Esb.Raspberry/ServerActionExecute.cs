using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Bmf.Shared.Esb;
using Bmf.Shared.Esb.Types;
using log4net.Util;
using Raspberry.IO.Components.Controllers.Pca9685;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.InterIntegratedCircuit;
using UnitsNet;

namespace Esb.Raspberry
{
    public class ServerActionExecuteBase
    {
        private static I2cDriver _i2CDriver;
        protected static IPwmDevice Device;

        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.ServerActionExecute");
        protected static ConcurrentDictionary<PwmChannel, ChannelSettings> Servos = new ConcurrentDictionary<PwmChannel, ChannelSettings>();

        static ServerActionExecuteBase()
        {
            EnsureI2CDevice();
        }

        private static void EnsureI2CDevice()
        {
            if (_i2CDriver == null)
            {
                try
                {
                    Log.Info("Creating i2cDevice");
                    var options = new
                    {
                        SdaPin = ConnectorPin.P1Pin03,
                        SclPin = ConnectorPin.P1Pin05,
                        DeviceAddress = 0x40,
                        PwmFrequency = Frequency.FromHertz(60)
                    };

                    _i2CDriver = new I2cDriver(options.SdaPin.ToProcessor(), options.SclPin.ToProcessor());
                    Device = new Pca9685Connection(_i2CDriver.Connect(options.DeviceAddress));
                    Device.SetPwmUpdateRate(options.PwmFrequency);
                }
                catch (InvalidOperationException e)
                {
                    Log.Error("Failed to connect? Do you have a Pca9685 IC attached to the i2c line and powered on?", e);
                }
            }
        }

    }

    public class ServerActionExecute : ServerActionExecuteBase, IReceiver<ServoExecuteMessage>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, ServoExecuteMessage message)
        {
            Log.Info($"Got Action {message.Action} for servo {message.Channel}");
            ChannelSettings ChannelSettings;
            if (Servos.TryGetValue(message.Channel, out ChannelSettings))
            {
                switch (message.Action)
                {
                    case ServoAction.Home:
                        ChannelSettings.Home();
                        break;
                    case ServoAction.Increase:
                        ChannelSettings.Increase();
                        break;
                    case ServoAction.Decrease:
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

    public class ServerActionExecuteAddServo : ServerActionExecuteBase, IReceiver<CreateServoMessage>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, CreateServoMessage message)
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