using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfServer.ErrorsHandling;
using WcfServer.Properties;
using WcfServer.Services;
using WcfServer.Services.Chats;

namespace WcfServer
{
    public class ServerChatsInstance<MT, T> : ServerInstance<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        private ServiceHost chatServiceHost;


        public ServerChatsInstance()
            : this(new ChatsService<T>(), new ChatsBroadCastService())
        {
        }

        public ServerChatsInstance(DuplexService<T> service, IBroadCastContract broadcastService)
            : base(service, broadcastService)
        {
        }

        /// <summary>
        /// public for tests only
        /// </summary>
        public ChatsService<T> chatsService
        {
            get { return (ChatsService<T>) _service; }
        }

        public override void BeforeBroadCastOpen()
        {
            chatServiceHost = new ServiceHostWithChats<T>(chatsService, typeof (ChatMessagesService<T>),
                                                          new Uri(string.Format("http://localhost:{0}/{1}",
                                                                                Settings.Default.ServerMexPort,
                                                                                Namespaces.ChatMessagesServicePath)));
            AddCommonEndpoints(typeof (IChatMessagesContract), chatServiceHost,
                               Namespaces.ChatMessagesServicePath, true, true, Settings.Default.ServerListenPort)
                .Behaviors.Add(new ChatMessagesBehavior<T>());

            chatServiceHost.Open();
            base.BeforeBroadCastOpen();
        }

        protected override ServiceEndpoint AddCommonEndpoints(Type type, ServiceHost sh, string path, bool withMex,
                                                              bool authPolicy, int serverListenPort)
        {
            if (sh.Description.Behaviors.Find<ErrorBehavior<MT, T>>() == null)
                sh.Description.Behaviors.Add(new ErrorBehavior<MT, T>());

            return base.AddCommonEndpoints(type, sh, path, withMex, authPolicy, serverListenPort);
        }

        public override void BeforeServerClose()
        {
            if (chatServiceHost != null
                && chatServiceHost.State == CommunicationState.Opened)
            {
                chatServiceHost.Close();
            }
            chatServiceHost = null;
            base.BeforeServerClose();
        }
    }
}