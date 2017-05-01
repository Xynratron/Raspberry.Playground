using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Bmf.Shared.Utilities
{
    /// <summary>
    /// SpeedLogger is a small Utility Class to measure the speed of a piece of Code. 
    /// </summary>
    public class SpeedLogger : IDisposable
    {
        private static readonly Dictionary<string, Stopwatch> SpeedLogList = new Dictionary<string, Stopwatch>();

        /// <summary>
        /// Start logging
        /// </summary>
        public static void Start()
        {
#if DEBUG
            MethodBase mb = new StackTrace(true).GetFrame(1).GetMethod();
            Start(mb.ReflectedType.FullName + "." + mb.Name);
#endif
        }
        /// <summary>
        /// Stops the logging
        /// </summary>
        public static void Stop()
        {
#if DEBUG
            MethodBase mb = new StackTrace(true).GetFrame(1).GetMethod();
            Stop((mb.ReflectedType != null ? mb.ReflectedType.FullName : "") + "." + mb.Name);
#endif
        }

        /// <summary>
        /// Starts logging
        /// </summary>
        /// <param name="loggerName"></param>
        public static void Start(string loggerName)
        {
#if DEBUG
            Stopwatch sw = null;
            if (SpeedLogList.ContainsKey(loggerName))
            {
                sw = SpeedLogList[loggerName];
                sw.Reset();
            }
            else
            {
                sw = new Stopwatch();
                SpeedLogList.Add(loggerName, sw);
            }
            sw.Start();
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerName"></param>
        public static void Stop(string loggerName)
        {
#if DEBUG
            if (SpeedLogList.ContainsKey(loggerName))
            {
                Stopwatch sw = SpeedLogList[loggerName];
                Debug.WriteLine(">> {1,10:#0}ms <<-->> {0}", loggerName, sw.ElapsedMilliseconds);
                SpeedLogList.Remove(loggerName);
            }
#endif
        }

        #region IDisposable Member
        private string Caller;
        private Stopwatch sw;
        /// <summary>
        /// 
        /// </summary>
        public SpeedLogger()
        {
#if DEBUG
            sw = new Stopwatch();
            MethodBase mb = new StackTrace(true).GetFrame(1).GetMethod();
            Caller = (mb.ReflectedType != null ? mb.ReflectedType.FullName : "") + "." + mb.Name;
            sw.Start();
#endif
        }

        public SpeedLogger(string suffix)
        {
#if DEBUG
            sw = new Stopwatch();
            MethodBase mb = new StackTrace(true).GetFrame(1).GetMethod();
            Caller = (mb.ReflectedType != null ? mb.ReflectedType.FullName : "") + "." + mb.Name + "__" + suffix;
            sw.Start();
#endif
        }

        ~SpeedLogger()
        {
#if DEBUG
            Write();
#endif
        }
        public void Dispose()
        {
#if DEBUG
            Write();
            GC.SuppressFinalize(this);
#endif
        }
        private void Write()
        {
#if DEBUG
            Debug.WriteLine(">> {1,10:#0}ms <<-->> {0}", Caller, sw.ElapsedMilliseconds);
#endif
        }

        #endregion

    }
}
