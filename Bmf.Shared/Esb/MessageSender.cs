using Bmf.Shared.Esb.Types;
using System;
using System.ServiceModel;
using Bmf.Shared.Properties;

namespace Bmf.Shared.Esb
{
    /// <summary>
    /// MessageSender is a small wrapper for sending messages to the Job-Server Mesh Network via Bmf Enterprise Service Bus. 
    /// It's purpose is a simple integration into other tool. So work which should be done on the servers can be transfered without referencing the complete 
    /// Bmf.Services.Esb.dll 
    /// 
    /// opens a new Endpoint <see ref="EndpointServiceClient" /> which is bound via BasicHttpBinding to the
    /// Address defined in <see ref="Settings.Default.JobserverUrl" />. The senderId from the setting Settings.Default.HostId
    /// is used.
    /// </summary>
    /// <example>  
    /// This sample shows how to call the <see cref="SendAndReceive"/> method.
    /// <code> 
    /// class TestClass  
    /// { 
    ///     static int Main()  
    ///     { 
    ///         //Why returning Zero?
    ///         return GetZero(); 
    ///     } 
    /// } 
    /// </code> 
    /// </example> 
    public static class MessageSender
    {        
        /// <summary>
        /// Send some Message with type of
        /// </summary>
        /// <typeparam name="TRequest">This is the Type of the requested message</typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="message"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public static TResponse SendAndReceive<TRequest, TResponse>(TRequest message, Guid? transactionId = null)
        {
            if (transactionId == null)
                transactionId = Guid.NewGuid();

            var _senderId = Settings.Default.HostId;

            var messageEnvelope = new Envelope
            {
                Sender = _senderId,
                Priority = Priority.normal,
                Processing = new ProcessingInformation
                {
                    CreatedOn = DateTime.Now,
                    FinishedOn = DateTime.MinValue
                },
                TransactionId = transactionId.Value,
                MessageType = typeof(TRequest),
                Message = message
            };

            var binding = new BasicHttpBinding { Name = "binding1" };
            var endPoint = new EndpointAddress(Settings.Default.JobserverUrl);
            using (var server = new EndpointServiceClient(binding, endPoint))
            {
                messageEnvelope.AddHeader(string.Format("{0} - Sending Message to {1}", _senderId != Guid.Empty ? _senderId.ToString() : Environment.MachineName, endPoint));
                var maxMessageSize = (int)((BasicHttpBinding)server.Endpoint.Binding).MaxReceivedMessageSize;

                var result = server.ReceiveAndSendMessage(messageEnvelope);

                //Hard Typecast for getting an Exception if the arguments are not valid
                return (TResponse)result.Message;

            }
        }

        public static void Send<T>(T message, Guid? transactionId = null)
        {
            if (transactionId == null)
                transactionId = Guid.NewGuid();
            var _senderId = Settings.Default.HostId;
            var messageEnvelope = new Envelope
            {
                Sender = _senderId,
                Priority = Priority.normal,
                Processing = new ProcessingInformation
                {
                    CreatedOn = DateTime.Now,
                    FinishedOn = DateTime.MinValue
                },
                TransactionId = transactionId.Value,
                MessageType = typeof(T),
                Message = message
            };
            
            var binding = new BasicHttpBinding { Name = "binding1" };
            var endPoint = new EndpointAddress(Settings.Default.JobserverUrl);
            using (var server = new EndpointServiceClient(binding, endPoint))
            {
                messageEnvelope.AddHeader(string.Format("{0} - Sending Message to {1}", _senderId != Guid.Empty ? _senderId.ToString() : Environment.MachineName, endPoint));
                var maxMessageSize = (int)((BasicHttpBinding)server.Endpoint.Binding).MaxReceivedMessageSize;

                // RZ TEST
                maxMessageSize = maxMessageSize - 10000;

                foreach (Envelope envelope in Envelope.GetChunks(messageEnvelope, maxMessageSize))
                {
                    server.ReceiveMessage(envelope);
                }
            }

        }
    }
}
