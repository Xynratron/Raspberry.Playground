using Raspberry.IO.GeneralPurpose;

namespace Esb.Raspberry
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