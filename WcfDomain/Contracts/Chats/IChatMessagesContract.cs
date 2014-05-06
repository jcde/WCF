using System.ServiceModel;

namespace WcfDomain.Contracts.Chats
{
    /// <summary>
    /// sessionful - Service Instances for every Chat Room
    /// Header should contain Chat Room name
    /// every method returns string (null - success, not null - failure description)
    /// </summary>
    [ServiceContract(Namespace = Namespaces.Services)]
    public interface IChatMessagesContract
    {
        /// <summary>
        /// Client wants to join Chat Room
        /// </summary>
        /// <param name="joinedUniqueKey">when chat request is approved, then this is chat INITIATOR's key</param>
        [OperationContract]
        string JoinRoom(string joinedUniqueKey);

        /// <param name="leftUniqueKey">when chat request is rejected, then this is chat INITIATOR's key</param>
        [OperationContract]
        string LeaveRoom(string leftUniqueKey);

        /// <summary>
        /// Client wants to send message into Chat Room
        /// </summary>
        [OperationContract]
        string SendMessage(string message);

        /// <summary>
        /// DirtyProperties contains list of properties to be modified
        /// </summary>
        /// <permission>only Moderator has access</permission>
        [OperationContract]
        string ManagePermissions(ChatRoom room);
    }
}