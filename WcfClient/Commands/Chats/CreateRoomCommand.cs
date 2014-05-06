using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class CreateRoomCommand<MT, T> : ChatsCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public CreateRoomCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public CreateRoomCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }


        protected override object ExecuteCommand(object message)
        {
            if (message is string)
                CheckError(MainChannel.CreateRoom(new ChatRoom {Name = (string) message}));
            if (message is ChatRoom)
                CheckError(MainChannel.CreateRoom((ChatRoom) message));
            return null;
        }
    }
}