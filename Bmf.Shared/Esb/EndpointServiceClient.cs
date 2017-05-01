using System;
using System.ServiceModel;
using Bmf.Shared.Esb.Types;

namespace Bmf.Shared.Esb
{
    public class EndpointServiceClient : System.ServiceModel.ClientBase<IEndpointService>, IEndpointService, IDisposable {
        
        public EndpointServiceClient() {
        }
        
        public EndpointServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName) {
            }
        
        public EndpointServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress) {
            }
        
        public EndpointServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress) {
            }
        
        public EndpointServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress) {
            }
        
        public void ReceiveMessage(Envelope envelope) 
        {
            Channel.ReceiveMessage(envelope);
        }

        public Envelope ReceiveAndSendMessage(Envelope envelope)
        {
            return Channel.ReceiveAndSendMessage(envelope);
        }

        ~EndpointServiceClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (_disposed) return;
            if (isDisposing)
            {
                try
                {
                    if (State != CommunicationState.Faulted)
                    {
                        Close();
                    }
                }
                finally
                {
                    if (State != CommunicationState.Closed)
                    {
                        Abort();
                    }
                }

            }
            _disposed = true;
        }
    }
}