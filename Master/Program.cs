using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bmf.Shared.Esb;
using Esb.Raspberry;
using Raspberry.IO.Components.Controllers.Pca9685;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            SteeringTest();
        }

        private static void SteeringTest()
        {
            MessageSender.Send(new CreateServoMessage(PwmChannel.C0, 325, 525, 0, 5));
            MessageSender.Send(new CreateServoMessage(PwmChannel.C14, 200, 700, 0, 25));
            MessageSender.Send(new CreateServoMessage(PwmChannel.C15, 200, 700, 0, 25));

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
