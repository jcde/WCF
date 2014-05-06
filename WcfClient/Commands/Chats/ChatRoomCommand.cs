using System;
using System.Collections;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows.Threading;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Chats;
using WcfDomain.Contracts.Clients;
using AppConfiguration.Wpf;

namespace WcfClient.Commands.Chats
{
    public abstract class ChatRoomCommand<MT, T> : ChatsCommand<MT, T>
        where MT : class, IChatsContract
        where T : class, IChatsBroadCastContract
    {
        protected ChatRoomCommand()
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="noNewBinding"></param>
        protected ChatRoomCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        /// <summary>
        /// can be used in ExecuteCommand and in CanExecuteCommand
        /// </summary>
        protected ChatRoom ChatRoom
        {
            get
            {
                ChatRoom dc = null;
                if (SourceControl != null)
                    if (!SourceControl.Dispatcher.CheckAccess())
                        SourceControl.Dispatcher
                            .Invoke(DispatcherPriority.Normal,
                                    (Action) delegate { dc = TreeHelper.FindDataContext<ChatRoom>(SourceControl); });
                    else
                        dc = TreeHelper.FindDataContext<ChatRoom>(SourceControl);
                if (dc == null)
                    dc = Client.ChatRoomSelected;
                return dc;
            }
        }

        private string ChatRoomNameSelected
        {
            get
            {
                return ChatRoom != null
                           ? ChatRoom.Name
                           : Client.ChatRoomNameSelected;
            }
        }

        protected bool IsClientActiveInChatRoom
        {
            get
            {
                return new ArrayList(ChatRoom.ActiveClients) // ArrayList to compare with string
                    .Contains(Client.UniqueKey);
            }
        }

        protected override bool CanExecuteCommand(object par)
        {
            return base.CanExecuteCommand(par)
                   && ChatRoom != null;
        }

        protected override sealed object ExecuteCommand(object par)
        {
            MessageHeader header = MessageHeader.CreateHeader(
                ChatRoom.ChatRoomNameHeader, Namespaces.HeaderNamespace, ChatRoomNameSelected);
            MessageHeader headerToken = MessageHeader.CreateHeader(
                Connection.TokenHeader, Namespaces.HeaderNamespace, Client.Token);

            using (new OperationContextScope((IContextChannel) ChatMessagesChannel))
            {
                OperationContext.Current.OutgoingMessageHeaders.Add(header);
                OperationContext.Current.OutgoingMessageHeaders.Add(headerToken);
                CheckError(ExecuteChatCommand(par));
            }
            return null;
        }

        protected abstract string ExecuteChatCommand(object par);
    }
}