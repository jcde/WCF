using System.Collections.Generic;
using System.ServiceModel;
using WcfDomain.Contracts.Clients;

namespace WcfDomain.Contracts.Chats
{
    [ServiceContract(Namespace = Namespaces.Services)]
    public interface IChatsBroadCastContract : IBroadCastContract
    {
        [OperationContract(IsOneWay = true)]
        void ChatRooms(List<ChatRoom> rooms);

        /// <summary>
        /// New visible Chat Room was created
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ChatRoomAdded(ChatRoom room, User initiator, List<ChatRoom> rooms);

        /// <summary>
        /// Client was joined Chat Room
        /// initiator is notified also
        /// </summary>
        /// <param name="chatRoom">contains up-to-date ActiveClients</param>
        [OperationContract(IsOneWay = true)]
        void ChatUserJoined(Connection joined, ChatRoom chatRoom);

        /// <summary>
        /// initiator is notified also
        /// </summary>
        /// <param name="chatRoom">contains up-to-date ActiveClients</param>
        [OperationContract(IsOneWay = true)]
        void ChatUserLeft(Connection left, ChatRoom chatRoom);

        /// <summary>
        /// Someone (initiator) wants to chat or join
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ChatJoinReceived(User initiator, JoinType joinType, string chatRoomName);

        /// <summary>
        /// Someone (initiator) rejected chat or join 
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ChatJoinRejected(User initiator, string reason);

        /// <summary>
        /// Message was sent to Chat Room
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ChatMessageSent(Connection initiator, string chatRoomName, string message);

        [OperationContract(IsOneWay = true)]
        void ChatUserStatusChanged(User initiator, ChatUserStatus newStatus);
    }
}