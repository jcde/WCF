using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    public class SendMessageCommand<MT, T> : ChatRoomCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public SendMessageCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        public SendMessageCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            return base.CanExecuteCommand(par)
                   && IsClientActiveInChatRoom
                   && par is string && !string.IsNullOrEmpty((string) par);
        }

        protected override string ExecuteChatCommand(object message)
        {
            return ChatMessagesChannel.SendMessage(message == null ? null : message.ToString());
        }
    }
}