namespace Bmf.Shared.Esb
{
    /// <summary>
    /// The information-levell of an message which is stored in the logging <see cref="Bmf.Shared.Esb.ILogger"/>
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// maximum detailed information for debugging
        /// </summary>
        Debug, 
        /// <summary>
        /// Messages only for informational purpose
        /// </summary>
        Information, 
        /// <summary>
        /// Warning messages, the process may fail, some needed informations are not accessible, but the process can run etc.
        /// </summary>
        Warning, 
        /// <summary>
        /// An error occured. 
        /// </summary>
        Error, 
        /// <summary>
        /// It is no longer possible for the process to do anything
        /// </summary>
        Fatal
    }
}