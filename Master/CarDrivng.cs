using System;
using System.Timers;
using Bmf.Shared.Esb;
using Esb.Raspberry;
using Raspberry.Helper;
using Raspberry.IO.Components.Controllers.Pca9685;
using Raspberry.IO.GeneralPurpose;
using SharpDX.DirectInput;

namespace Master
{
    internal static class CarDrivng
    {
        internal static void Start()
        {
            MessageSender.Send(new EnableI2CChannel(PwmChannel.C0, 325, 525, 0, 5));
            MessageSender.Send(new EnableI2CChannel(PwmChannel.C14, 200, 700, 0, 25));
            MessageSender.Send(new EnableI2CChannel(PwmChannel.C15, 200, 700, 0, 25));

            //EN_M0 = 4  # servo driver IC CH4
            //EN_M1 = 5  # servo driver IC CH5
            MessageSender.Send(new EnableI2CChannel(PwmChannel.C4, 0, 4000, 0, 400));
            MessageSender.Send(new EnableI2CChannel(PwmChannel.C5, 0, 4000, 0, 400));

            /*Motor0_A = 11  # pin11
              Motor0_B = 12  # pin12
              Motor1_A = 13  # pin13s
              Motor1_B = 15  # pin15
            */

            GamePad();
        }
        private static void GamePad()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad,
                DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                    DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    SetJoyButtonStatus(state.Offset, state.Value);
                    Console.WriteLine(state);
                }
            }
        }

        private static void SetJoyButtonStatus(JoystickOffset button, int value)
        {
            if (button == JoystickOffset.Buttons1 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C14;
                servoExecuteMessage.Action = I2CChannelAction.Decrease;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons3 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C14;
                servoExecuteMessage.Action = I2CChannelAction.Increase;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons0 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C15;
                servoExecuteMessage.Action = I2CChannelAction.Increase;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons2 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C15;
                servoExecuteMessage.Action = I2CChannelAction.Decrease;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.X)
            {
                if (JoystickOffsetXLastState != value)
                {
                    JoystickOffsetXLastState = value;
                    if (value == 32511)
                    {
                        JoystickOffsetXTimer.Stop();
                        JoystickOffsetXTimer = null;
                    }
                    else
                    {
                        if (JoystickOffsetXTimer != null)
                            JoystickOffsetXTimer.Stop();

                        JoystickOffsetXTimer = new Timer
                        {
                            Interval = 25,
                            AutoReset = true
                        };
                        JoystickOffsetXTimer.Elapsed += (sender, args) =>
                        {
                            if (JoystickOffsetXLastState == 0)
                                MessageSender.Send(new ServoExecuteMessage
                                {
                                    Channel = PwmChannel.C0,
                                    Action = I2CChannelAction.Decrease
                                });
                            else
                            {
                                MessageSender.Send(new ServoExecuteMessage
                                {
                                    Channel = PwmChannel.C0,
                                    Action = I2CChannelAction.Increase
                                });
                            }
                        };
                        JoystickOffsetXTimer.Start();
                    }
                }
            }


            if (button == JoystickOffset.Y)
            {
                switch (value)
                {
                    case 0:
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin17, true));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin18, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin27, true));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin22, false));
                        break;
                    case 65535:
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin17, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin18, true));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin27, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin22, true));
                        break;
                    default:
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin17, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin18, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin27, false));
                        MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin22, false));
                        break;
                }
            }

            if (button == JoystickOffset.Buttons5 && value == 128)
            {
                MessageSender.Send(new ServoExecuteMessage
                {
                    Channel = PwmChannel.C4,
                    Action = I2CChannelAction.Decrease
                });
                MessageSender.Send(new ServoExecuteMessage
                {
                    Channel = PwmChannel.C5,
                    Action = I2CChannelAction.Decrease
                });
            }

            if (button == JoystickOffset.Buttons7 && value == 128)
            {
                MessageSender.Send(new ServoExecuteMessage
                {
                    Channel = PwmChannel.C4,
                    Action = I2CChannelAction.Increase
                });
                MessageSender.Send(new ServoExecuteMessage
                {
                    Channel = PwmChannel.C5,
                    Action = I2CChannelAction.Increase
                });
            }
        }

        private static int JoystickOffsetXLastState = 32511;
        private static Timer JoystickOffsetXTimer;

        private static void SteeringTest()
        {
            var currentchannel = PwmChannel.C0;

            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).KeyChar != 'x')
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = currentchannel;

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        servoExecuteMessage.Action = I2CChannelAction.Decrease;
                        break;
                    case ConsoleKey.RightArrow:
                        servoExecuteMessage.Action = I2CChannelAction.Increase;
                        break;
                    case ConsoleKey.UpArrow:
                        servoExecuteMessage.Action = I2CChannelAction.Home;
                        break;
                    case ConsoleKey.S:
                        currentchannel = PwmChannel.C0;
                        continue;
                    case ConsoleKey.B:
                        currentchannel = PwmChannel.C14;
                        continue;
                    case ConsoleKey.T:
                        currentchannel = PwmChannel.C15;
                        continue;
                    default:
                        continue;
                }

                MessageSender.Send(servoExecuteMessage);
            }
        }

    }
}