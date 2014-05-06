using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace WcfDomain.Contracts
{
    /// <summary>
    /// server and client use broadcasting in the same manner
    /// </summary>
    public class BroadcastChannels<T> : Channels, IDisposable where T : IBroadCastContract
    {
        public readonly T Callback;

        public ChannelFactory<T> ChannelBroadcastFactory;
        public Action OpenChannelAction;
        private ICommunicationObject _channel;

        public BroadcastChannels(string server, int port, T callback)
            : this(server, port, callback, null)
        {
        }

        public BroadcastChannels(string server, int port, T callback, Action action)
        {
            Callback = callback;
            OpenChannelAction = action;

            var address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/{2}",
                                                            server, port,
                                                            Namespaces.BroadcastServicePath));
            try
            {
                ChannelBroadcastFactory = new ChannelFactory<T>(GetBinding(), address);
            }
            catch
            {
            }

#if DEBUG
            ServiceEndpoint endpoint = ChannelBroadcastFactory.Endpoint;
            CallbackDebugBehavior cdb = endpoint.Behaviors.Find<CallbackDebugBehavior>()
                                        ?? new CallbackDebugBehavior(true);
            cdb.IncludeExceptionDetailInFaults = true;
            if (endpoint.Behaviors.Find<CallbackDebugBehavior>() == null)
                endpoint.Behaviors.Add(cdb);
#endif
        }

        public T Channel
        {
            get { return Open(ref _channel, ChannelBroadcastFactory, OpenChannelAction); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            CloseChannel(ref _channel);
            CloseFactory(ref ChannelBroadcastFactory);
        }

        #endregion
    }
}