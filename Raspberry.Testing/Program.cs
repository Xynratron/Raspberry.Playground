using System;
using Bmf.Shared.Esb;
using Raspberry.Helper;
using Raspberry.IO.GeneralPurpose;

namespace Raspberry.Testing
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Program");

            BlinkSomeLed.Start();

            Console.WriteLine("Ending Program");
        }
    }
}