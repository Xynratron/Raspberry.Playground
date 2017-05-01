using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bmf.Shared.Esb;
using Raspberry.Helper;
using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Testing
{
    public class RaspiFluent
    {
        public RaspiFluent()
        {
            _IsRemote = Environment.OSVersion.Platform != PlatformID.Unix;
        }
        public ProcessorPin Pin { get; set; }

        private bool _IsRemote;

        public RaspiFluent AsRemote()
        {
            _IsRemote = true;
            return this;
        }

        public RaspiFluent On()
        {
            if (_IsRemote)
                MessageSender.Send(new GpioSetStatus(Pin, true));
            else
                new GpioPinServer().SetStatus(new GpioSetStatus(Pin, true));
            return this;
        }
        public RaspiFluent Off()
        {
            if (_IsRemote)
                MessageSender.Send(new GpioSetStatus(Pin, false));
            else
                new GpioPinServer().SetStatus(new GpioSetStatus(Pin, false));
            return this;
        }
    }

    public static class RaspiFluentInit
    {
        public static RaspiFluent AsRemote(this ProcessorPin pin)
        {
            return new RaspiFluent {Pin = pin}.AsRemote();
        }

        public static RaspiFluent On(this ProcessorPin pin)
        {
            return new RaspiFluent {Pin = pin}.On();
        }
        public static RaspiFluent Off(this ProcessorPin pin)
        {
            return new RaspiFluent { Pin = pin }.Off();
        }
    }
}
