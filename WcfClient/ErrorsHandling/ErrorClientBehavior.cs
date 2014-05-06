using System;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfClient.ErrorsHandling
{
    public class ErrorClientBehavior : IEndpointBehavior
    {
        private readonly IErrorHandler _errorHandler;

        public ErrorClientBehavior(IErrorHandler errorHandler)
        {
            if (errorHandler == null)
            {
                throw new ArgumentNullException();
            }
            _errorHandler = errorHandler;
        }

        #region IEndpointBehavior Members

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            Collection<IErrorHandler> handlers = clientRuntime.CallbackDispatchRuntime.ChannelDispatcher.ErrorHandlers;
            if (!handlers.Contains(_errorHandler))
                handlers.Add(_errorHandler);

            foreach (IClientMessageInspector inspector in clientRuntime.MessageInspectors)
                if (inspector is ExceptionMarshallingMessageInspector)
                    return;
            clientRuntime.MessageInspectors.Add(new ExceptionMarshallingMessageInspector());
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            Collection<IErrorHandler> handlers = endpointDispatcher.ChannelDispatcher.ErrorHandlers;
            if (!handlers.Contains(_errorHandler))
                handlers.Add(_errorHandler);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}