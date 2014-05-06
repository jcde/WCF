using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class JoinRoomCommand<MT, T> : ChatRoomCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public JoinRoomCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public JoinRoomCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            return base.CanExecuteCommand(par)
                   && !IsClientActiveInChatRoom;
        }

        protected override string ExecuteChatCommand(object par)
        {
            string r = ChatMessagesChannel.JoinRoom(par is string ? (string) par : Client.UniqueKey);
            if (r == Resources.ApprovalNeeded)
            {
            }
            return r;
        }
    }
}