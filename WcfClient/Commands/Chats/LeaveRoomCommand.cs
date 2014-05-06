using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class LeaveRoomCommand<MT, T> : ChatRoomCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public LeaveRoomCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public LeaveRoomCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            return base.CanExecuteCommand(par)
                   && IsClientActiveInChatRoom;
        }

        protected override string ExecuteChatCommand(object par)
        {
            return ChatMessagesChannel.LeaveRoom(par is string ? (string) par : Client.UniqueKey);
        }
    }
}