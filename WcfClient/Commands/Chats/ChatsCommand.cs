using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public abstract class ChatsCommand<MT, T> : ChatsClientCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        protected ChatsCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        protected ChatsCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }
    }
}