using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using ServiceModelEx;
using WcfDomain.Contracts.Chats;
using WcfServer.Services;
using WcfServer.Services.Chats;

namespace WcfServer.ErrorsHandling
{
    /// <summary>
    /// allows for service to send serialized Exceptions to client
    /// uses ErrorHandler and ExceptionMarshallingMessageInspector on client
    /// </summary>
    public class ErrorBehavior<MT, T> : IServiceBehavior
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        private readonly ErrorHandler<T> _errorHandler;

        public ErrorBehavior()
        {
            _errorHandler = new ErrorHandler<T>();

            // creates cache with methods of contract t
            //does not work on 3.0.0.0 MethodsManager.GetMethods(typeof (MT));
        }

        #region IServiceBehavior Members

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription,
                                                    ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase is ServiceHostWithChats<T>)
                _errorHandler.Service = ((ServiceHostWithChats<T>) serviceHostBase).ChatsService;
            else
                _errorHandler.Service = ((ServiceHost) serviceHostBase).SingletonInstance as DuplexService<T>;

            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(_errorHandler);
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
                                         Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}