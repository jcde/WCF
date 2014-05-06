using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;
using WcfDomain.Threads;
using Resources = WcfServer.Properties.Resources;

namespace WcfServer.Services.Chats
{
    /// <summary>
    /// not sessionful - methods of one Service Instance
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        Namespace = Namespaces.Services,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatsService<T> : DuplexService<T>, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        internal readonly Dictionary<ChatRoom, ChatMessagesService<T>> Rooms
            = new Dictionary<ChatRoom, ChatMessagesService<T>>();

        /// <summary>
        /// if empty, then it is meant to be Available
        /// </summary>
        private readonly Dictionary<User, ChatUserStatus> _userStatuses
            = new Dictionary<User, ChatUserStatus>();

        public ChatsService()
        {
            CreateRoom(new ChatRoom
                           {
                               Name = ChatRoom.ChatRoomSystem,
                           }, false);
        }

        #region IChatsContract Members

        public List<ChatRoom> RoomList()
        {
            return (List<ChatRoom>) ComplexMonitor.CopyFrom(Rooms.Keys);
        }

        public string CreateRoom(ChatRoom newRoom)
        {
            return CreateRoom(newRoom, true);
        }

        public string StartChat(User otherUser)
        {
            if (SenderUser == otherUser)
                return Resources.UserWrong;

            List<Connection> otherClients = GetClientsNotDelphi(otherUser);
            if (otherClients.Count == 0)
                return Resources.UserNotConnected;

            bool needApprovalForIn = false;
            if (_userStatuses.ContainsKey(otherUser))
            {
                switch (_userStatuses[otherUser])
                {
                    case ChatUserStatus.AvailableOutOnly:
                    case ChatUserStatus.NotAvailable:
                        return Resources.UserNotAvailable;
                    case ChatUserStatus.NeedApprovalForIn:
                        needApprovalForIn = true;
                        break;
                }
            }

            //begin... Chat Room with joined Sender is created
            ChatRoom room = (from e in Rooms
                             where !e.Key.IsPublic && !e.Key.IsAutoAccepted
                                   && (e.Key.Moderator == SenderUser || e.Key.Moderator == otherUser)
                                   && e.Key.ActiveUsers.Except(new[] {SenderUser, otherUser}).Count() == 0
                             select e.Key).FirstOrDefault();

            if (room != null)
            {
                string s = null;
                if (!room.ActiveUsers.Contains(SenderUser))
                    s = Rooms[room].JoinRoom(Sender, true, true);

                // approval is not needed if otherUser is already joined the same room as Sender
                if (room.ActiveUsers.Contains(otherUser) || !needApprovalForIn)
                {
                    if (!room.ActiveUsers.Contains(otherUser))
                        s += Rooms[room].JoinRoom(Sender, otherClients[0], true, true);
                    return s ?? Resources.ChatRoomReused;
                }
            }

            if (room == null)
            {
                room = new ChatRoom
                           {
                               Name = ChatRoom.DirectChatRoomName(SenderUser, otherUser),
                               IsAutoAccepted = false,
                               IsPublic = false,
                               AllowedUsers = new List<User> {SenderUser, otherUser},
                           };
                CreateRoom(room);
            }
            //end..... Chat Room with joined Sender is created

            if (needApprovalForIn)
            {
                bool otherClientNotified = false;
                otherClients.ForEach(a =>
                                         {
                                             try
                                             {
                                                 a.BroadcastChannel<T>().ChatJoinReceived(SenderUser,
                                                                                          JoinType.ApproveNeeded,
                                                                                          room.Name);
                                                 otherClientNotified = true;
                                             }
                                             catch (SystemException)
                                             {
                                                 PuntUser(a, "bad connection");
                                             }
                                         });
                if (!otherClientNotified)
                    return Resources.UserNotConnected;

                lock (Rooms[room]._userStatuses)
                    Rooms[room]._userStatuses[otherUser] = ChatUser4RoomStatus.WaitingOwnApprove;

                return WcfDomain.Contracts.Chats.Resources.ApprovalNeeded;
            }
            return Rooms[room].JoinRoom(Sender, otherClients[0], true, true);
        }

        public string ChangeStatus(ChatUserStatus newStatus)
        {
            _userStatuses[SenderUser] = newStatus;
            BroadcastsChannel.ChatUserStatusChanged(SenderUser, newStatus);
            return null;
        }

        #endregion

        private string CreateRoom(ChatRoom newRoom, bool broadCast)
        {
            if (FindRoom(newRoom.Name) != null)
                return string.Format(Resources.ChatRoomExists, newRoom.Name);

            newRoom.StartedDate = DateTime.UtcNow;
            newRoom.Moderator = SenderUser;
            var messagesService = new ChatMessagesService<T>(this, newRoom);

            Rooms.Add(newRoom, messagesService);
            messagesService.JoinRoom(Sender, true, broadCast);

            if (broadCast)
                if (newRoom.IsPublic)
                    BroadcastsChannel.ChatRoomAdded(newRoom, SenderUser, RoomList());
                else
                {
                    GetClients(newRoom.CanSeePrivate, false)
                        .ForEach(a =>
                                     {
                                         try
                                         {
                                             a.BroadcastChannel<T>().ChatRoomAdded(newRoom, SenderUser, RoomList());
                                         }
                                         catch (SystemException)
                                         {
                                             PuntUser(a, "bad connection");
                                         }
                                     });
                }

            return null;
        }

        internal ChatRoom FindRoomWithCheck(string name)
        {
            ChatRoom r = FindRoom(name);
            if (r == null)
            {
                throw new ApplicationException(string.Format(Resources.ChatRoomWrong, name));
            }
            return r;
        }

        internal ChatRoom FindRoom(string name)
        {
            return (from e in Rooms
                    where e.Key.Name == name
                    select e.Key).SingleOrDefault();
        }

        protected override void OnConnectNotifyAfter(Connection c)
        {
            base.OnConnectNotifyAfter(c);
            if (c.ClientType != ClientType.SendAndReceiver_WithoutChat)
            {
                SenderBroadcastsChannel.ChatRooms(RoomList());
                Rooms[FindRoom(ChatRoom.ChatRoomSystem)].JoinRoom(c, true, true);
            }
        }

        protected override void OnDisconnectedBefore(Connection c)
        {
            base.OnDisconnectedBefore(c);
            if (c.ClientType != ClientType.SendAndReceiver_WithoutChat)
            {
                bool notify = SenderBroadcastsChannel != null
                              && ((ICommunicationObject) SenderBroadcastsChannel).State == CommunicationState.Opened;
                if (notify)
                    SenderBroadcastsChannel.ChatRooms(null);
                new List<ChatMessagesService<T>>(Rooms.Values).ForEach(e => e.LeaveRoom(c, notify));
            }
        }

        public override void Clear()
        {
            base.Clear();
            _userStatuses.Clear();
            foreach (var s in Rooms.Values)
                s.Clear();
            foreach (ChatRoom r in new List<ChatRoom>(Rooms.Keys))
                if (r.Name != ChatRoom.ChatRoomSystem)
                    Rooms.Remove(r);
                else
                {
                    foreach (Connection c in Clients)
                        lock (r.ActiveClients)
                            if (!r.ActiveClients.Contains(c))
                                r.ActiveClients.Add(c);
                }
        }
    }
}