using System;
using System.Collections.Generic;

namespace Bmf.Shared.Esb
{
    /// <summary>
    /// Interface for an internal logging implementation.
    /// </summary>
    public interface ILogger
    {
        void Debug(Guid transactionId, string message, int finishedPercent = 0);
        void Info(Guid transactionId, string message, int finishedPercent = 0);
        void Warn(Guid transactionId, string message, int finishedPercent = 0);
        void Error(Guid transactionId, string message, int finishedPercent = 0);
        void Fatal(Guid transactionId, string message, int finishedPercent = 0);
        List<LogEntry> FindLogEntries(int lastEntyId = 0, Guid? transaction = null, DateTime? firstOccurence = null, int maxItems = 0);
    }
}