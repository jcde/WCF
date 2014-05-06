using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class UsersCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public UsersCommand()
        {
        }

        public UsersCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            return MainChannel.RequestUsers();
        }
    }
}