using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Bmf.Shared.Caching
{
    public static class IndependedRepository
    {
        private const int MaxCachingTimeInSeconds = 600;
        private const int MaxOnHoldTimeInSeconds = 60;
        private static readonly Timer CleanupTimer;

        private static readonly Dictionary<string, ICacheOptions> LocalStorage = new Dictionary<string, ICacheOptions>();

        static IndependedRepository()
        {
            CleanupTimer = new Timer();
            CleanupTimer.Elapsed += CleanupTimerElapsed;
            CleanupTimer.Interval = 1000;
            CleanupTimer.Enabled = true;
        }

        private static void CleanupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CleanupTimer.Enabled = false;
            CleanUp();
            CleanupTimer.Enabled = true;
        }

        private static void CleanUp()
        {
            lock (LocalStorage)
            {
                //using (new SpeedLogger("_Stopped"))
                //{
                //System.Diagnostics.Debug.WriteLine("Repository Cleanup Started");
                // Zerstören da kein Reload
                var zuLöschen =
                    LocalStorage.Where(o => o.Value.IsExpired)
                        //(!o.Value.IsValid && !o.Value.ReloadAfterExpire) || ((o.Value.CreatedOn + o.Value.MaxCachingTime) < DateTime.Now))
                        .Select(o => o.Key)
                        .ToList();
                foreach (var key in zuLöschen)
                {
                    LocalStorage.Remove(key);
                }

                // Zerstören und danach wieder Laden
                var reload = LocalStorage.Where(o => o.Value.MustReload).ToList();
                foreach (var item in reload)
                {
                    //LocalStorage.Remove(item.Key);
                    //var x = item.Value;
                    item.Value.ResetCreatetOn();
                    //GetCachingObject(x);
                }
                //}
            }
        }

        public static ICacheOptions GetCachingObject(ICacheOptions options)
        {
            options.Validate();
            lock (LocalStorage)
            {
                ICacheOptions result;
                if (LocalStorage.TryGetValue(options.Key, out result))
                {
                    if (!options.Reset)
                    {
                        result.LastAccess = DateTime.Now;
                        return result;
                    }
                    LocalStorage.Remove(options.Key);
                }
                options.LastAccess = DateTime.Now;
                if (options.MaxCachingTime == TimeSpan.Zero)
                    options.MaxCachingTime = TimeSpan.FromSeconds(MaxCachingTimeInSeconds);
                if (options.MaxOnHoldTime == TimeSpan.Zero)
                    options.MaxOnHoldTime = TimeSpan.FromSeconds(MaxOnHoldTimeInSeconds);
                LocalStorage.Add(options.Key, options);
                return options;
            }
        }
    }
}