using Bmf.Shared.Esb;
using Raspberry.Helper;
using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Testing
{
    //http://www.holgerschurig.de/de/raspi-lauflicht/
    //https://techmike1985.wordpress.com/2013/06/10/74hc595-schieberegister-grundlagen/
    internal class ShiftRegisterIoPlaying
    {
        public static void Start()
        {
            Initialize();

            AllOff();

            System.Threading.Thread.Sleep(1000);

            SetWithSingleBitValues();

            System.Threading.Thread.Sleep(1000);

            SetASingleByte();

            System.Threading.Thread.Sleep(1000);

            MoveUpAndDown();

            System.Threading.Thread.Sleep(1000);

            CountUp();
            CountDown();
        }

        static ProcessorPin data = ProcessorPin.Pin10;
        static ProcessorPin latch = ProcessorPin.Pin08;
        static ProcessorPin clock = ProcessorPin.Pin11;
        private static void MoveUpAndDown()
        {
            var d = (byte)0x01;
            for (var i = 1; i < 8; i++)
            {
                SendByte(d);
                WriteLatch();
                d = (byte)(d << 1);
            }
            for (var i = 8; i > 0; i--)
            {
                SendByte(d);
                WriteLatch();
                d = (byte)(d >> 1);
            }
        }

        private static void CountDown()
        {
            for (int i = 255; i >= 0; i--)
            {
                SendByte((byte)i);
                WriteLatch();
            }
        }

        private static void CountUp()
        {
            for (int i = 0; i < 256; i++)
            {
                SendByte((byte)i);
                WriteLatch();
            }
        }

        private static void SetASingleByte()
        {
            SendByte(0xCD);
            WriteLatch();
        }

        private static void SetWithSingleBitValues()
        {
            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            SendBit(true);
            SendBit(false);
            WriteLatch();
        }

        private static void AllOff()
        {
            SendByte(0x0);
            WriteLatch();
        }

        private static void Initialize()
        {
            MessageSender.Send(new GpioSetStatus(data, false));
            MessageSender.Send(new GpioSetStatus(latch, false));
            MessageSender.Send(new GpioSetStatus(clock, false));
        }

        private static void SendByte(byte data)
        {
            var a = (int)data;
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