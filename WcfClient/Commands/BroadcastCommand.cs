using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class BroadcastCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public BroadcastCommand()
        {
        }

        public BroadcastCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            BroadcastChannel.BroadcastMessage(message == null ? null : message.ToString(), Client.UniqueKey);
            return null;
        }
    }
}