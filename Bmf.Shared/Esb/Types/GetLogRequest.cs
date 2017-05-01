using System;

namespace Bmf.Shared.Esb.Types
{
    /// <summary>
    /// Sends a Request to the Jobserver to return Entries from the Logfile depending on the filtering below. 
    /// </summary>
    public class GetLogRequest
    {
        public int LastEntryId { get; set; }
        /// <summary>
        /// Specify a Transaction for the Log
        /// </summary>
        public Guid? TransactionId { get; set; }
        /// <summary>
        /// Return all Entries which are older than this DateTime
        /// </summary>
        public DateTime? EntriesSince { get; set; }
        /// <summary>
        /// The Maximum amount of Entries
        /// </summary>
        public int MaxEntries { get; set; }
    }
}