using System;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using AppConfiguration.Localization;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;
using WcfDomain.Contracts.Security;
using WcfServer.Performance;
using WcfServer.Services;

namespace WcfServer
{
    /// <summary>
    /// messages to Main service and from Broadcasting serverice are inspected here
    /// </summary>
    public class MessageInspector<T> : IDispatchMessageInspector, IEndpointBehavior
        where T : class, IBroadCastContract
    {
        internal PerfCounters BroadcastPerf;

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, IClientChannel channel,
                                          InstanceContext instanceContext)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.DefaultLanguage);
            if (BroadcastPerf == null)
            {
                object instance = ((ServiceHost) instanceContext.Host).SingletonInstance;
                var service = (DuplexService<T>) instance;
                Connection sender = service.GetClient(channel);

                if (sender != null) // may be null till connect
                {
                    if (sender.ClientType == ClientType.SendAndReceiver_WithoutChat
                        && typeof (IChatsContract).IsAssignableFrom(ClaimsPolicy.CurrentContact()))
                    {
                        throw new PermissionException("Delphi client does not support WCF chats");
                    }

                    sender.LastReceiveDate = DateTime.UtcNow;
                }

                service.PerfCounters.Count(PerfCounters.Messages2ServerPerSecond);
                ClaimsPolicy.ServerCheck();
            }
            else
            {
                BroadcastPerf.Count(PerfCounters.MessagesFromServerPerSecond);
                ClaimsPolicy.ServerCheck();
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        #endregion

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint,
                                         BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            throw new Exception("The EndpointBehaviorMessageInspector is not used in client applications.");
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MessageInspector<T>
                                                                         {BroadcastPerf = BroadcastPerf});
        }

        public void Validate(ServiceEndpoint serviceEndpoint)
        {
            return;
        }

        #endregion
    }
}