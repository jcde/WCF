using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class ChangeStatusCommand<MT, T> : ChatsCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public ChangeStatusCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public ChangeStatusCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object par)
        {
            if (par is ChatUserStatus)
                CheckError(MainChannel.ChangeStatus((ChatUserStatus) par));
            return null;
        }
    }
}