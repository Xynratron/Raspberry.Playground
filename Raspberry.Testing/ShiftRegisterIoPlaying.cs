using System.Threading;
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

            Thread.Sleep(500);

            SetWithSingleBitValues();

            Thread.Sleep(500);

            SetASingleByte();

            Thread.Sleep(500);

            MoveUpAndDown();

            Thread.Sleep(500);

            CountUp();
            CountDown();
        }

        static RaspiFluent data = ProcessorPin.Pin10.AsFluent();
        static RaspiFluent latch = ProcessorPin.Pin08.AsFluent();
        static RaspiFluent clock = ProcessorPin.Pin11.AsFluent();

        private static void MoveUpAndDown()
        {
            for (var i = 1; i < 256; i *= 2)
            {
                SendByte((byte)i);
                Thread.Sleep(100);
            }
            
            for (byte i = 128; i > 0; i = (byte)(i >> 1))
            {
                SendByte(i);
                Thread.Sleep(100);
            }
            SendByte(0x0);
        }

        private static void CountDown()
        {
            for (int i = 255; i >= 0; i--)
            {
                SendByte((byte)i);
                Thread.Sleep(25);
            }
        }

        private static void CountUp()
        {
            for (int i = 0; i < 256; i++)
            {
                SendByte((byte)i);
                Thread.Sleep(25);
            }
        }

        private static void SetASingleByte()
        {
            SendByte(0xCD);
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
        }

        private static void Initialize()
        {
            data.Off();
            latch.Off();
            clock.Off();
        }

        private static void SendByte(byte data)
        {
            SendBit((data & 0b00000001) != 0);
            SendBit((data & 0b00000010) != 0);
            SendBit((data & 0b00000100) != 0);
            SendBit((data & 0b00001000) != 0);
            SendBit((data & 0b00010000) != 0);
            SendBit((data & 0b00100000) != 0);
            SendBit((data & 0b01000000) != 0);
            SendBit((data & 0b10000000) != 0);
            WriteLatch();
        }

        private static void SendBit(bool value)
        {
            data.Set(value);
            clock.On().Off();
        }

        private static void WriteLatch()
        {
            latch.On().Off();
        }      
    }
}