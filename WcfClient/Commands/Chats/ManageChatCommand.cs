using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class ManageChatCommand<MT, T> : ChatRoomCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public ManageChatCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public ManageChatCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            return base.CanExecuteCommand(par)
                   && ChatRoom.Moderator == Client.User;
        }

        protected override string ExecuteChatCommand(object par)
        {
            var room = (ChatRoom) par;
            string r = ChatMessagesChannel.ManagePermissions(room);
            if (r == null)
                room.DirtyProperties.Clear();
            return r;
        }
    }
}