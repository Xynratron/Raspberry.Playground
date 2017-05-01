using System.Collections.Concurrent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
 
namespace Bmf.Shared.Esb.Types
{
    [DataContract]
    [DebuggerDisplay("Id={MessageId}")]
    public class Envelope
    {
        public Envelope()
        {
            MessageId = Guid.NewGuid();
            Priority = Priority.normal;
            Processing = new ProcessingInformation {CreatedOn = DateTime.Now};
            JunkedTransfer = ChunkedTransport.none;
        }

        [DataMember]
        public ChunkedTransport JunkedTransfer { get; set; }

        [DataMember]
        public int ChunkId { get; set; }

        [DataMember]
        public Guid Sender { get; set; }

        [DataMember]
        public Guid MessageId { get; private set; }

        [DataMember]
        public Priority Priority { get; set; }

        [DataMember]
        public Guid TransactionId { get; set; }

        //ProcesssingInforamtion ist intern für den Dispatcher, Wenn z.B. Started übertragen würde, 
        //dann ist die Information im Storrage falsch und der Dispatcher würde diese Message nicht 
        //mehr zur Bearbeitung freigeben
        //[DataMember]
        private ProcessingInformation _processing;
        public ProcessingInformation Processing
        {
            get { return _processing ?? (_processing = new ProcessingInformation {CreatedOn = DateTime.Now}); }
            set { _processing = value; }
        }

        [DataMember]
        public string Headers { get; private set; }
        public void AddHeader(string headerValue)
        {
            Headers += string.Format("{0} - {1}{2}", DateTime.Now, headerValue, Environment.NewLine);
        }
        [DataMember]
        private string MessageTypeString { get; set; }

        [DataMember]
        private string JsonMessageString { get; set; }

        private readonly static ConcurrentDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>(); 

        //Nicht übertragen
        private Type _messageType;
        public Type MessageType
        {
            get
            {
                if (_messageType == null)
                {
                    if (!string.IsNullOrWhiteSpace(MessageTypeString))
                    {
                        if (!ResolvedTypes.TryGetValue(MessageTypeString, out _messageType))
                        {
                            _messageType = ResolvedTypes.GetOrAdd(MessageTypeString, s =>
                                Type.GetType(MessageTypeString) ??
                                Type.GetType(MessageTypeString, name => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(z => z.FullName == name.FullName), null)
                            );
                        }
                    }
                }
                return _messageType;
            }
            set
            {
                if (_messageType == value) return;
                MessageTypeString = value.AssemblyQualifiedName;
                _messageType = value;
            }
        }
        
        private dynamic _message;
        public dynamic Message
        {
            get
            {
                if (_message == null)
                {
                    if (JsonMessageString != null)
                    {
                        _message = JsonConvert.DeserializeObject(JsonMessageString, MessageType);
                    }
                }
                return _message;
            }
            set
            {
                if (_message == null || _message != value)
                {
                    MessageType = value.GetType();
                    JsonMessageString = JsonConvert.SerializeObject(value);
                    _message = value;
                }
            }
        }

        //only estimated, must be adjusted if Envelope ist changed
        public const int Overhead = 16 + 16 + 4 + 8 + 8 + 8 + 4 + 4 + 4 + 4 + 4 + 640 + 16
            + 16 + 1024
            + 4096; 
        
        private int Size
        {
            get
            {
                return (JsonMessageString ?? string.Empty).Length + (Headers ?? string.Empty).Length + (MessageTypeString??string.Empty).Length + Overhead;
            }
        }

        public static Envelope ResembleMessages(IEnumerable<Envelope> chunks)
        {
            chunks = chunks.ToList();
            var first = chunks.First();

            string type = first.Message;
            var headers = chunks.Where(o => o.JunkedTransfer == ChunkedTransport.chunkHeader).OrderBy(o => o.ChunkId).Aggregate(string.Empty, (current, envelope) => (string)(current + envelope.Message));

            var data = chunks.Where(o => o.JunkedTransfer == ChunkedTransport.chunkData).OrderBy(o => o.ChunkId).Aggregate(string.Empty, (current, envelope) => (string)(current + envelope.Message));
            data += chunks.First(o => o.JunkedTransfer == ChunkedTransport.end).Message;

            return new Envelope
            {
                ChunkId = 0,
                JunkedTransfer = ChunkedTransport.none,
                Priority = first.Priority,
                Sender = first.Sender,
                MessageId = first.MessageId,
                TransactionId = first.TransactionId,
                Headers = headers,
                MessageTypeString = type,
                JsonMessageString = data
            };
        }

        public static IEnumerable<Envelope> GetChunks(Envelope message, int maxMessageSize)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            maxMessageSize = maxMessageSize - Envelope.Overhead;
            if (maxMessageSize < 1)
                throw new ArgumentOutOfRangeException("maxMessageSize", string.Format("The needed Overhead ({0}) for a Message exceeds the MaximumMessageSize ({1}). The Messages will not be able to send.", Overhead, maxMessageSize));
            if (message.Message == null)
                throw new NullReferenceException("The Message can not be null");

            if (message.Size < maxMessageSize)
                return new List<Envelope>(new[] { message });

            int chunks = 0;
            var result = new List<Envelope>();
            var start = new Envelope
            {
                ChunkId = chunks++,
                JunkedTransfer = ChunkedTransport.begin,
                Priority = message.Priority,
                Sender = message.Sender,
                MessageId = message.MessageId,
                MessageType = typeof(string),
                Message = message.MessageTypeString,
                TransactionId = message.TransactionId
            };
            result.Add(start);

            var data = message.Headers??string.Empty; //, message.MessageTypeString, message.JsonMessageString);

            while (data.Length > maxMessageSize)
            {
                var chunk = new Envelope
                {
                    ChunkId = chunks++,
                    JunkedTransfer = ChunkedTransport.chunkHeader,
                    Priority = message.Priority,
                    Sender = message.Sender,
                    MessageId = message.MessageId,
                    MessageType = typeof(string),
                    Message = data.Substring(0, maxMessageSize),
                    TransactionId = message.TransactionId
                };
                data = data.Substring(maxMessageSize);
                result.Add(chunk);
            }
            result.Add(new Envelope
            {
                ChunkId = chunks++,
                JunkedTransfer = ChunkedTransport.chunkHeader,
                Priority = message.Priority,
                MessageId = message.MessageId,
                Sender = message.Sender,
                MessageType = typeof(string),
                Message = data,
                TransactionId = message.TransactionId
            });

            data = message.JsonMessageString;

            while (data.Length > maxMessageSize)
            {
                var chunk = new Envelope
                {
                    ChunkId = chunks++,
                    JunkedTransfer = ChunkedTransport.chunkData,
                    Priority = message.Priority,
                    Sender = message.Sender,
                    MessageId = message.MessageId,
                    MessageType = typeof(string),
                    Message = data.Substring(0, maxMessageSize),
                    TransactionId = message.TransactionId
                };
                data = data.Substring(maxMessageSize);
                result.Add(chunk);
            }
            var end = new Envelope
            {
                ChunkId = chunks++,
                JunkedTransfer = ChunkedTransport.end,
                Priority = message.Priority,
                MessageId = message.MessageId,
                Sender = message.Sender,
                MessageType = typeof(string),
                Message = data,
                TransactionId = message.TransactionId
            };

            result.Add(end);
            return result;
        }
    }
}