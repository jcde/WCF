using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using WcfDomain.Contracts;

namespace WcfClient.ErrorsHandling
{
    /// <summary>
    /// here exceptions thrown inside of broadcast channel are catched
    /// </summary>
    public class ClientErrorHandler<MT, T> : IErrorHandler
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        private readonly ClientInstance<MT, T> client;
        private readonly ChannelFactory channelFactory;

        public ClientErrorHandler(ClientInstance<MT, T> o, ChannelFactory factory)
        {
            client = o;
            channelFactory = factory;
        }

        #region IErrorHandler Members

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            fault = null;
        }

        public bool HandleError(Exception error)
        {
            //ClientCommand<MT, T>.CloseClient(client);
            //client.CreateFactories();
            //client.TryAutoConnect();

            Debug.WriteLine(string.Format("client {0} handled error {1} (channelFactory state {2})",
                                          client.ApplicationID, error.Message, channelFactory.State));
            return true;
        }

        #endregion
    }
}