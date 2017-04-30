using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bmf.Shared.Esb;
using Esb.Raspberry;
using Raspberry.IO.GeneralPurpose;

namespace Master
{
    class Program
    {
        static ProcessorPin data = ProcessorPin.Pin10;
        static ProcessorPin latch = ProcessorPin.Pin08;
        static ProcessorPin clock = ProcessorPin.Pin11;

        static void Main(string[] args)
        {
            MessageSender.Send(new GpioSetStatus(data, false));
            MessageSender.Send(new GpioSetStatus(latch, false));
            MessageSender.Send(new GpioSetStatus(clock, false));

            SendByte(0x0);
            WriteLatch();

            System.Threading.Thread.Sleep(1000);

            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            WriteLatch();

            System.Threading.Thread.Sleep(1000);

            SendByte(0xCD);
            WriteLatch();

            System.Threading.Thread.Sleep(1000);

            for (int i = 0; i < 256; i++)
            {
                SendByte((byte)i);
                WriteLatch();
                Console.WriteLine(i);
                //System.Threading.Thread.Sleep(10);
            }
            for (int i = 255; i >= 0; i--)
            {
                SendByte((byte)i);
                WriteLatch();
                //System.Threading.Thread.Sleep(10);
            }


            //SendBit(true);
            //SendBit(false);
            //SendBit(true);
            //SendBit(false);
            //SendBit(true);
            //SendBit(false);
            //SendBit(true);
            //SendBit(false);

            //System.Threading.Thread.Sleep(2000);
            //Blink();

            //WriteLatch();
            //Blink();
            //System.Threading.Thread.Sleep(1000);
            //Blink();
        }

        private static void SendByte(byte data)
        {
            var a = (int) data;
            for (int i = 0; i < 8; i++)
            {
                SendBit((a & 0x01) == 0x01);
                a = a >> 1;
            }
        }

        private static void SendBit(bool value)
        {
            MessageSender.Send(new GpioSetStatus(data, value));
            MessageSender.Send(new GpioSetStatus(clock, true));
            MessageSender.Send(new GpioSetStatus(clock, false));
        }

        private static void WriteLatch()
        {
            MessageSender.Send(new GpioSetStatus(latch, true));
            MessageSender.Send(new GpioSetStatus(latch, false));
        }

        private static void Blink()
        {
            MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin13, true));
            System.Threading.Thread.Sleep(100);
            MessageSender.Send(new GpioSetStatus(ProcessorPin.Pin13, false));
        }
    }
}