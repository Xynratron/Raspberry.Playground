using System;

namespace Bmf.Shared.Esb
{
    /// <summary>
    /// generic interface for asynchronous message sending synchronous sending and receiving
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Sends a message and waits for the job server. The result of the work will directly returned
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        TResponse SendAndReceive<TRequest, TResponse>(TRequest message, Guid? transactionId = null);
        /// <summary>
        /// Sends the specified message for asynchronous processing on the server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        void Send<T>(T message, Guid? transactionId = null);
    }
}