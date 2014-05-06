using System;
using System.ServiceModel;
using WcfDomain.Contracts.Chats;

namespace WcfServer.Services.Chats
{
    internal class ServiceHostWithChats<T> : ServiceHost
        where T : class, IChatsBroadCastContract
    {
        public ChatsService<T> ChatsService;

        public ServiceHostWithChats(ChatsService<T> chatsService,
                                    Type type, Uri uri) : base(type, uri)
        {
            ChatsService = chatsService;
        }
    }
}