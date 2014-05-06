using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public class KillCommand<MT, T> : DisconnectCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public KillCommand()
        {
        }

        public KillCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object par)
        {
            return ExecuteCommand(par, true);
        }
    }
}