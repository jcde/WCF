using System;
using System.ServiceModel;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;
using WcfClient.Commands.Chats;

namespace WcfClient
{
    public class ClientChatsInstance<MT, T> : ClientInstance<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public ClientChatsInstance()
        {
        }

        public ClientChatsInstance(string appID)
            : base(appID, new ChatsCommandsManager<MT, T>())
        {
        }

        public ClientChatsInstance(string server, int port, string appID, bool autoConnect)
            : base(server, port, appID, autoConnect, new ChatsCommandsManager<MT, T>())
        {
        }

        private ICommunicationObject _chatMessagesChannel;
        private ChannelFactory<IChatMessagesContract> ChatMessagesChannelFactory;

        public IChatMessagesContract ChatMessagesChannel
        {
            get
            {
                if (Settings.ClientType != ClientType.SendAndReceiver_WithoutChat)
                    return Channels.Open(ref _chatMessagesChannel, ChatMessagesChannelFactory);
                throw new ApplicationException("chats are disabled");
            }
        }

        public override void AfterCreateChannels(NetTcpBinding binding)
        {
            if (Settings.ClientType != ClientType.SendAndReceiver_WithoutChat)
                ChatMessagesChannelFactory = new ChannelFactory<IChatMessagesContract>
                    (binding, new EndpointAddress(GetServerAddress(Namespaces.ChatMessagesServicePath)));
            base.AfterCreateChannels(binding);
        }

        public override void BeforeCloseChannel()
        {
            if (Settings.ClientType != ClientType.SendAndReceiver_WithoutChat)
            {
                Channels.CloseChannel(ref _chatMessagesChannel);
                Channels.CloseFactory(ref ChatMessagesChannelFactory);
            }
            base.BeforeCloseChannel();
        }

        public string ChatRoomNameSelected;
        public ChatRoom ChatRoomSelected { get; set; }
    }
}