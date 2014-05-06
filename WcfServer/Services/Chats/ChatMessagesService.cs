using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;
using WcfDomain.Threads;
using Resources = WcfServer.Properties.Resources;

namespace WcfServer.Services.Chats
{
    /// <summary>
    /// sessionful - Service Instances for every Chat Room
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall,
        MaxItemsInObjectGraph = 1000000,
        Namespace = Namespaces.Services)]
    public sealed class ChatMessagesService<T> : IChatMessagesContract
        where T : class, IChatsBroadCastContract
    {
        private readonly DuplexService<T> _mainService;

        /// <summary>
        /// should be accessed and modified through lock 
        /// </summary>
        private readonly ChatRoom _room;

        /// <summary>
        /// should be accessed and modified through lock 
        /// </summary>
        internal readonly Dictionary<User, ChatUser4RoomStatus> _userStatuses
            = new Dictionary<User, ChatUser4RoomStatus>();

        public ChatMessagesService(DuplexService<T> mainService, ChatRoom room)
        {
            _mainService = mainService;
            _room = room;
        }

        #region Join and Leave room

        public string JoinRoom(string joinedUniqueKey)
        {
            Connection joined = _mainService.GetClient(joinedUniqueKey, false);
            if (_room.Moderator == Initiator.User && Initiator.ClientUniqueKey != joinedUniqueKey
                && joined != null)
                lock (_userStatuses)
                    if (_userStatuses.ContainsKey(joined.User)
                        && _userStatuses[joined.User] == ChatUser4RoomStatus.WaitingModeratorApproveToJoin)
                    {
                        _userStatuses.Remove(joined.User);
                    }
            if (Initiator.ClientUniqueKey != joinedUniqueKey)
                lock (_userStatuses)
                    if (_userStatuses.ContainsKey(Initiator.User))
                        if (_userStatuses[Initiator.User] == ChatUser4RoomStatus.WaitingOwnApprove)
                        {
                            _userStatuses.Remove(Initiator.User);
                            return JoinRoom(Initiator, Initiator, false, true);
                        }
            if (_room.Moderator != null
                && _room.Moderator != Initiator.User && Initiator.ClientUniqueKey != joinedUniqueKey)
                return Resources.ModeratorJoin;

            return JoinRoom(Initiator, joined, false, true);
        }

        public string LeaveRoom(string leftUniqueKey)
        {
            Connection left = _mainService.GetClient(leftUniqueKey, false);

            if (_room.Moderator == Initiator.User && Initiator.ClientUniqueKey != leftUniqueKey
                && left != null)
            {
                lock (_userStatuses)
                    if (_userStatuses.ContainsKey(left.User))
                    {
                        if (_userStatuses[left.User] == ChatUser4RoomStatus.WaitingModeratorApproveToJoin)
                        {
                            _userStatuses.Remove(left.User);
                            _mainService.Broadcast(left.User)
                                .ForEach(a => a.BroadcastChannel<T>()
                                                  .ChatJoinRejected(Initiator.User, Resources.RejectedJoin));
                            return null;
                        }
                    }
            }
            if (Initiator.ClientUniqueKey != leftUniqueKey && left != null)
                lock (_userStatuses)
                    if (_userStatuses.ContainsKey(Initiator.User))
                        if (_userStatuses[Initiator.User] == ChatUser4RoomStatus.WaitingOwnApprove)
                        {
                            _userStatuses.Remove(Initiator.User);
                            _mainService.Broadcast(left.User)
                                .ForEach(a => a.BroadcastChannel<T>()
                                                  .ChatJoinRejected(Initiator.User, Resources.RejectedJoin));
                            return null;
                        }

            if (Initiator.ClientUniqueKey != leftUniqueKey
                && (_room.Moderator == null || _room.Moderator != Initiator.User))
                return Resources.ModeratorLeave;

            return LeaveRoom(left);
        }

        internal string JoinRoom(Connection initiator, bool withoutAllowanceCheck, bool broadCast)
        {
            return JoinRoom(initiator, initiator, withoutAllowanceCheck, broadCast);
        }

        internal string JoinRoom(Connection initiator, Connection joined, bool withoutAllowanceCheck, bool broadCast)
        {
            if (initiator != null && joined != null)
            {
                bool f;
                lock (_room.AllowedUsers)
                    f = !withoutAllowanceCheck && _room.Moderator != initiator.User
                        && !_room.AllowedUsers.Contains(joined.User);
                if (f)
                {
                    lock (_room.DeniedUsers)
                    {
                        if (_room.DeniedUsers.Contains(joined.User))
                        {
                            return Resources.AccessDenied;
                        }
                    }
                    if (!_room.IsPublic)
                        return Resources.PrivateRoomAllowedUsersOnly;

                    if (!_room.IsAutoAccepted)
                    {
                        lock (_userStatuses)
                            if (_userStatuses.ContainsKey(joined.User)
                                && _userStatuses[joined.User] == ChatUser4RoomStatus.WaitingModeratorApproveToJoin)
                            {
                                return WcfDomain.Contracts.Chats.Resources.ApprovalNeeded;
                            }

                        if (_room.Moderator == null)
                            return Resources.ModeratorNull;

                        List<Connection> moderatorClients = _mainService.Broadcast(_room.Moderator);
                        bool moderatorNotified = false;
                        moderatorClients.ForEach(a =>
                                                     {
                                                         try
                                                         {
                                                             a.BroadcastChannel<T>()
                                                                 .ChatJoinReceived(Initiator.User,
                                                                                   JoinType.ApproveNeeded,
                                                                                   _room.Name);
                                                             moderatorNotified = true;
                                                         }
                                                         catch (SystemException)
                                                         {
                                                             _mainService.PuntUser(a, "bad connection");
                                                         }
                                                     });
                        if (!moderatorNotified)
                            return Resources.UserNotConnected;

                        lock (_userStatuses)
                            _userStatuses[joined.User] = ChatUser4RoomStatus.WaitingModeratorApproveToJoin;
                        return WcfDomain.Contracts.Chats.Resources.ApprovalNeeded;
                    }
                }

                lock (_room.ActiveClients)
                {
                    if (_room.ActiveClients.Contains(joined))
                        return string.Format(Resources.ChatUserAlreadyJoined, joined, _room);
                    _room.ActiveClients.Add(joined);
                }

                if (broadCast)
                {
                    _mainService.GetClients(_room.ActiveUsersWithModerator, false)
                        .ForEach(a =>
                                     {
                                         try
                                         {
                                             a.BroadcastChannel<T>().ChatUserJoined(joined,
                                                                                    (ChatRoom)
                                                                                    ComplexMonitor.CopyFrom(_room));
                                         }
                                         catch (SystemException)
                                         {
                                             _mainService.PuntUser(a, "bad connection");
                                         }
                                     });
                }
            }
            else
                return string.Format("Initiator or joiner to room {0} was not determined", _room);

            return null;
        }

        public string LeaveRoom(Connection left)
        {
            return LeaveRoom(left, true);
        }

        public string LeaveRoom(Connection left, bool notify)
        {
            if (left == null)
                return "Lefter was not determined";

            List<Connection> beNotified = null;
            lock (_room.ActiveClients)
            {
                if (notify)
                    beNotified = new List<Connection>(_room.ActiveClients);
                _room.ActiveClients.Remove(left);
            }
            if (beNotified != null)
                beNotified.ForEach(a =>
                                       {
                                           try
                                           {
                                               a.BroadcastChannel<T>().ChatUserLeft(left,
                                                                                    (ChatRoom)
                                                                                    ComplexMonitor.CopyFrom(_room));
                                           }
                                           catch (SystemException)
                                           {
                                               _mainService.PuntUser(a, "bad connection");
                                           }
                                       });
            return null;
        }

        #endregion

        private Connection Initiator
        {
            get
            {
                var token = OperationContext.Current.IncomingMessageHeaders.GetHeader<Guid>(Connection.TokenHeader,
                                                                                            Namespaces.HeaderNamespace);
                if (token != Guid.Empty)
                {
                    lock (_mainService.Tokens)
                        if (_mainService.Tokens.ContainsKey(token))
                            return _mainService.Tokens[token];
                }
                throw new ApplicationException(Resources.AccessDenied);
            }
        }

        #region IChatMessagesContract Members

        public string SendMessage(string message)
        {
            if (!_room.ActiveUsers.Contains(Initiator.User))
                return Resources.NotJoined;
            _room.ActiveClients.ForEach(a =>
                                            {
                                                try
                                                {
                                                    a.BroadcastChannel<T>().ChatMessageSent(
                                                        Initiator, _room.Name, message);
                                                }
                                                catch (SystemException)
                                                {
                                                    _mainService.PuntUser(a, "bad connection");
                                                }
                                            }
                );
            return null;
        }

        public string ManagePermissions(ChatRoom room)
        {
            if (Initiator.User != _room.Moderator)
                return Resources.AccessDenied;
            foreach (string p in room.DirtyProperties)
            {
                PropertyInfo pr = typeof (ChatRoom).GetProperty(p);
                using (new ComplexMonitor(_room))
                {
                    pr.SetValue(_room, pr.GetValue(room, null), null);
                }
            }
            return null;
        }

        #endregion

        public void Clear()
        {
            _userStatuses.Clear();
        }
    }
}