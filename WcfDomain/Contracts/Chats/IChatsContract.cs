using System.Collections.Generic;
using System.ServiceModel;
using WcfDomain.Contracts.Clients;

namespace WcfDomain.Contracts.Chats
{
    /// <summary>
    /// not sessionful
    /// </summary>
    [ServiceContract(Namespace = Namespaces.Services,
        CallbackContract = typeof (IChatsBroadCastContract))]
    public interface IChatsContract : IContract
    {
        /// <summary>
        /// Get visible Chat Rooms
        /// </summary>
        [OperationContract]
        List<ChatRoom> RoomList();

        /// <summary>
        /// Client wants to create and join Chat Room 
        /// </summary>
        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationContract]
        string CreateRoom(ChatRoom newRoom);

        /// <summary>
        /// Client wants to chat with other Client directly
        /// </summary>
        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationContract]
        string StartChat(User otherUser);

        /// <summary>
        /// Client wants to change his status
        /// </summary>
        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationContract]
        string ChangeStatus(ChatUserStatus newStatus);
    }
}