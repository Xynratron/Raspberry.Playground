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
        private static I2cDriver i2cDriver;
        protected static IPwmDevice device;

        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.ServerActionExecute");
        protected static ConcurrentDictionary<PwmChannel, ChannelSettings> _servos = new ConcurrentDictionary<PwmChannel, ChannelSettings>();

        static ServerActionExecuteBase()
        {
            EnsureI2CDevice();
        }

        private static void EnsureI2CDevice()
        {
            if (i2cDriver == null)
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

                    i2cDriver = new I2cDriver(options.SdaPin.ToProcessor(), options.SclPin.ToProcessor());
                    device = new Pca9685Connection(i2cDriver.Connect(options.DeviceAddress));
                    device.SetPwmUpdateRate(options.PwmFrequency);
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
        }
    }


    public class ServerActionExecuteAddServo : ServerActionExecuteBase, IReceiver<CreateServoMessage>
    {
        
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, CreateServoMessage message)
        {
            if (_servos.TryAdd(message.Channel,
                new ChannelSettings(device, message.Channel)
                {
                    MaxPwm = message.MaxPwm,
                    MinPwm = message.MinPwm,
                    Offset = message.Offset,
                    Step = message.Step
                }))
            {
                Log.Info($"Added Servo for Channel {message.Channel}.");
            }
            else
            {
                Log.Info($"Servo for Channel {message.Channel} could not be added. It may allready exists.");
            }
        }
    }
}