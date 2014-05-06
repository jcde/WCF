using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using WcfDomain.Contracts.Chats;

namespace WcfServer.Services.Chats
{
    public class ChatMessagesBehavior<T> : IEndpointBehavior
        where T : class, IChatsBroadCastContract
    {
        #region IEndpointBehavior Members

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.InstanceProvider = new ChatMessagesFactory<T>();
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        #endregion
    }
}