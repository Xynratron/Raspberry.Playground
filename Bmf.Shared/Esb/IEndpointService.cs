using Bmf.Shared.Esb.Types;
using System.ServiceModel;

namespace Bmf.Shared.Esb
{
    [ServiceContract]
    public interface IEndpointService
    {
        [OperationContract]
        void ReceiveMessage(Envelope envelope);
        [OperationContract]
        Envelope ReceiveAndSendMessage(Envelope envelope);
    }
}