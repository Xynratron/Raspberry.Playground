using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Helper
{
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