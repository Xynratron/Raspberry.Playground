using System;

namespace Bmf.Shared.Esb
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid TransactionId { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public int FinishedPercent { get; set; }
    }
}