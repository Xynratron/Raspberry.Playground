using System;
using Bmf.Shared.Esb;
using Raspberry.Helper;
using System.Threading;
using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Testing
{
    internal static class BlinkSomeLed
    {
        internal static void Start(ProcessorPin pin = ProcessorPin.Pin17)
        {
            for (int i = 0; i < 10; i++)
            {
                pin.On();
                Thread.Sleep(100);
                pin.Off();
                Thread.Sleep(100);
            }
        }
    }
}