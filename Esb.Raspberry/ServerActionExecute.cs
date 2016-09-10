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
    public class ServerI2CActionsBase
    {
        private static I2cDriver _i2CDriver;
        protected static IPwmDevice Device;

        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.ServerI2CActions");
        protected static ConcurrentDictionary<PwmChannel, ChannelSettings> Servos = new ConcurrentDictionary<PwmChannel, ChannelSettings>();

        static ServerI2CActionsBase()
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

    public class GpioPinServer
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger("Esb.Raspberry.GpioPinServer");
        protected static GpioConnection GpioConnection = new GpioConnection();
    }

    public class GpioPinServerSetStatus : GpioPinServer, IReceiver<GpioSetStatus>
    {
        public void ReceiveMessage(IEnvironment environment, Envelope envelope, GpioSetStatus message)
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

    public class GpioSetStatus
    {
        public GpioSetStatus(ProcessorPin pin, bool state)
        {
            Pin = pin;
            State = state;
        }
        public ProcessorPin Pin { get; }
        public bool State { get; }
    }
}