using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfServer.Performance;

namespace WcfServer.Services.Chats
{
    public class ChatMessagesFactory<T> : IInstanceProvider
        where T : class, IChatsBroadCastContract
    {
        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (message == null)
                throw new ApplicationException("Empty message");
            var s = message.Headers.GetHeader<string>(ChatRoom.ChatRoomNameHeader, Namespaces.HeaderNamespace);
            ChatsService<T> service = ((ServiceHostWithChats<T>) instanceContext.Host).ChatsService;
            service.PerfCounters.Count(PerfCounters.ChatCommands2ServerPerSecond);

            ClaimsPolicy.ServerCheck();

            return service.Rooms[service.FindRoomWithCheck(s)];
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }

        #endregion
    }
}