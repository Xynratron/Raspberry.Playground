using System;
using System.Collections.Concurrent;
using Raspberry.IO.Components.Controllers.Pca9685;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.InterIntegratedCircuit;
using UnitsNet;

namespace Raspberry.Helper
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
}