using System;
using WcfDomain.Contracts.Chats;

namespace WcfClient.Commands.Chats
{
    /// <summary>
    /// calls commands without CommandBinding
    /// </summary>
    public class ChatsCommandsManager<MT, T> : CommandsManager<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        public override Type CommandType(ComEnum en)
        {
            var res = base.CommandType(en);
            if (res == null)
                switch (en)
                {
                    case ComEnum.CreateRoom:
                        return typeof(CreateRoomCommand<MT, T>);
                    case ComEnum.StartChat:
                        return typeof(StartChatCommand<MT, T>);
                    case ComEnum.ChangeStatus:
                        return typeof(ChangeStatusCommand<MT, T>);
                    case ComEnum.JoinRoom:
                        return typeof(JoinRoomCommand<MT, T>);
                    case ComEnum.LeaveRoom:
                        return typeof(LeaveRoomCommand<MT, T>);
                    case ComEnum.SendMessage:
                        return typeof(SendMessageCommand<MT, T>);
                    case ComEnum.ManageChat:
                        return typeof(ManageChatCommand<MT, T>);
                }
            return res;
        }
   }
}