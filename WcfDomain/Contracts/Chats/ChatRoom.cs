using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using WcfDomain.Contracts.Clients;

namespace WcfDomain.Contracts.Chats
{
    /// <summary>
    /// ChatRoom is highly modified by parallel threads object.
    /// locking:
    ///     List properties can be locked separately like lock (_room.ActiveClients)
    ///     whole object should by locked in whole like using (new ComplexMonitor(_room))
    /// if locking is too long and complex then you can copy ChatRoom instance 
    ///     by ComplexMonitor.CopyFrom(...)
    /// </summary>
    [DataContract(Namespace = Namespaces.Data)]
    public class ChatRoom
    {
        public const string ChatRoomNameHeader = "ChatRoomName";

        public static string ChatRoomSystem = "System Wide Chat Room";

        [DataMember] public List<Connection> ActiveClients = new List<Connection>();

        [DataMember] public List<User> AllowedUsers = new List<User>();

        [DataMember] public List<User> DeniedUsers = new List<User>();

        [DataMember] /// <summary>
            /// if true, then when Client not listed in Allowed Clients can join without Moderator approval. 
            /// if false, Moderator approval is required.
            /// </summary>
            public bool IsAutoAccepted = true;

        [DataMember] public bool IsPublic = true;
        [DataMember] public int MaxActiveUsers = 10;

        /// <summary>
        /// change on created Room is forbidden, because there are a lot of dependencies 
        /// </summary>
        [DataMember] public User Moderator;

        [DataMember] public string Name;
        [DataMember] public DateTime StartedDate;

        public object ID
        {
            get { return Name; }
            set { Name = (string) value; }
        }

        public List<User> CanSeePrivate
        {
            get
            {
                if (IsPublic)
                    throw new FaultException("should be private");
                return AllowedUsers.Except(DeniedUsers)
                    .Union(new[] {Moderator}).ToList();
            }
        }

        public List<User> ActiveUsers
        {
            get
            {
                return (from c in ActiveClients
                        select c.User).Distinct().ToList();
            }
        }

        public IEnumerable<User> ActiveUsersWithModerator
        {
            get
            {
                var beNotified = new List<User>(ActiveUsers);
                if (Moderator != null && !beNotified.Contains(Moderator))
                    beNotified.Add(Moderator);
                return beNotified;
            }
        }

        #region Entity maintain

        [DataMember] public readonly List<string> DirtyProperties = new List<string>();

        #endregion

        public override string ToString()
        {
            return string.Format("{0}", ID);
        }

        public static string DirectChatRoomName(User initiator, User otherUser)
        {
            return string.Format("Direct chat {0} with {1}", initiator, otherUser);
        }

        #region UniqueKey

        public override bool Equals(object obj)
        {
            if (obj is ChatRoom)
            {
                var ob = (ChatRoom) obj;
                return (ID == null && ob.ID == null || ID != null && ID.Equals(ob.ID));
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (ID ?? "").GetHashCode();
        }

        #endregion
    }
}