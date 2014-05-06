using System.Collections.Generic;
using System.ServiceModel;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;

namespace WcfServer.Services.Chats
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    //// have to allow calls to MainChannel from Broadcasted method, but doesn't work :(
        //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatsBroadCastService : BroadCastService<IChatsBroadCastContract>,
                                         IChatsBroadCastContract
    {
        #region Chats

        public void ChatRooms(List<ChatRoom> rooms)
        {
            FireEvent(rooms);
        }

        public void ChatRoomAdded(ChatRoom room, User initiator, List<ChatRoom> rooms)
        {
            FireEvent(room, initiator, rooms);
        }

        public void ChatUserJoined(Connection joined, ChatRoom chatRoom)
        {
            FireEvent(joined, chatRoom);
        }

        public void ChatUserLeft(Connection left, ChatRoom chatRoom)
        {
            FireEvent(left, chatRoom);
        }

        public void ChatJoinReceived(User initiator, JoinType joinType, string chatRoomName)
        {
            FireEvent(initiator, joinType, chatRoomName);
        }

        public void ChatJoinRejected(User initiator, string reason)
        {
            FireEvent(initiator, reason);
        }

        public void ChatMessageSent(Connection initiator, string chatRoomName, string message)
        {
            FireEvent(initiator, chatRoomName, message);
        }

        public void ChatUserStatusChanged(User initiator, ChatUserStatus newStatus)
        {
            FireEvent(initiator, newStatus);
        }

        #endregion
    }
}