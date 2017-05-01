using System;
using System.Linq;

namespace Bmf.Shared.Esb.Types
{
    public class PerfdataByTime
    {
        public PerfdataByTime()
        {
            Minutes = Enumerable.Repeat(0, 60).ToArray();
            Hours = Enumerable.Repeat(0, 24).ToArray();
        }
        public string Name { get; set; }
        public int[] Minutes { get; set; }
        public int[] Hours { get; set; }

        /// <summary>
        /// Adds the counter Information about runtime to the minutes and hours arrays.
        /// </summary>
        /// <param name="counter">The Counter object to add</param>
        /// <returns>true if successful, false if counter is to old.</returns>
        public bool TryAddValue(CountObject counter)
        {
            var durationInQueue = DateTime.Now - counter.Enqueued;
            if (durationInQueue > TimeSpan.FromDays(1))
                return false;

            //Arrays count from zero, but the first hour is stored in the minutes array and should remain zero
            if (durationInQueue.Hours > 0)
                Hours[durationInQueue.Hours]++;
            else
                Minutes[durationInQueue.Minutes]++;
            return true;
        }
    }
}
