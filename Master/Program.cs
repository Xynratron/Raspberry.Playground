﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Bmf.Shared.Esb;
using Esb.Raspberry;
using Raspberry.IO.Components.Controllers.Pca9685;
using SharpDX.DirectInput;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageSender.Send(new CreateServoMessage(PwmChannel.C0, 325, 525, 0, 5));
            MessageSender.Send(new CreateServoMessage(PwmChannel.C14, 200, 700, 0, 25));
            MessageSender.Send(new CreateServoMessage(PwmChannel.C15, 200, 700, 0, 25));

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
                servoExecuteMessage.Action = ServoAction.Decrease;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons3 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C14;
                servoExecuteMessage.Action = ServoAction.Increase;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons0 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C15;
                servoExecuteMessage.Action = ServoAction.Increase;
                MessageSender.Send(servoExecuteMessage);
            }

            if (button == JoystickOffset.Buttons2 && value == 128)
            {
                var servoExecuteMessage = new ServoExecuteMessage();
                servoExecuteMessage.Channel = PwmChannel.C15;
                servoExecuteMessage.Action = ServoAction.Decrease;
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
                                    Action = ServoAction.Decrease
                                });
                            else
                            {
                                MessageSender.Send(new ServoExecuteMessage
                                {
                                    Channel = PwmChannel.C0,
                                    Action = ServoAction.Increase
                                });
                            }

                        };
                        JoystickOffsetXTimer.Start();
                    }
                }
            }

            //if (button == JoystickOffset.X && value == 0)
            //{
            //    MessageSender.Send(new ServoExecuteMessage
            //    {
            //        Channel = PwmChannel.C0,
            //        Action = ServoAction.Decrease
            //    });
            //}

            //if (button == JoystickOffset.X && value == 65535)
            //{
            //    var servoExecuteMessage = new ServoExecuteMessage();
            //    servoExecuteMessage.Channel = PwmChannel.C0;
            //    servoExecuteMessage.Action = ServoAction.Increase;
            //    MessageSender.Send(servoExecuteMessage);
            //}
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
                        servoExecuteMessage.Action = ServoAction.Decrease; 
                        break;
                    case ConsoleKey.RightArrow:
                        servoExecuteMessage.Action = ServoAction.Increase;
                        break;
                    case ConsoleKey.UpArrow:
                        servoExecuteMessage.Action = ServoAction.Home;
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
