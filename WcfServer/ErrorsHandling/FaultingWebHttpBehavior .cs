using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using WcfDomain.Contracts;

namespace WcfServer.ErrorsHandling
{
    /// <summary>
    /// returns error messages as xml visible in internet browser
    /// </summary>
    public class FaultingHttpErrorBehavior : WebHttpBehavior, IErrorHandler
    {
        #region IErrorHandler Members

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            FaultCode faultCode = FaultCode.CreateSenderFaultCode(error.GetType().Name,
                                                                  Namespaces.ServiceExceptions);
            fault = Message.CreateMessage(version, faultCode, error.Message, null);
        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        #endregion

        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint,
                                                       EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(this);
        }
    }
}