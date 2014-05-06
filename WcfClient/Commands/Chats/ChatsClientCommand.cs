using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public abstract class ChatsClientCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IChatsContract // here we may type IContract if to update commandsManager.CommandType
        where T : class, IChatsBroadCastContract
    {
        protected new ClientChatsInstance<MT, T> Client
        {
            get { return (ClientChatsInstance<MT, T>) base.Client; }
        }

        protected ChatsClientCommand()
        { }
        protected ChatsClientCommand(bool calledFromCode)
            : base(calledFromCode)
        {
        }

        public IChatMessagesContract ChatMessagesChannel
        {
            get { return Client.ChatMessagesChannel; }
        }
    }
}