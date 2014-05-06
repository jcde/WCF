using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;

namespace WcfClient.Commands.Chats
{
    public class StartChatCommand<MT, T> : ChatsCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public StartChatCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public StartChatCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }


        protected override object ExecuteCommand(object par)
        {
            if (par is User)
                CheckError(MainChannel.StartChat((User) par));
            return null;
        }
    }
}