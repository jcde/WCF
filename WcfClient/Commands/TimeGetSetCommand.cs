using System;
using WcfDomain.Contracts;

namespace WcfClient.Commands
{
    public struct TimeGetSet
    {
        public DateTime DateTime;
        public bool IsExecute;
        public bool IsGet;
        public bool Success;
    }

    public class TimeGetSetCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public TimeGetSetCommand()
        {
        }

        public TimeGetSetCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override object ExecuteCommand(object message)
        {
            var gs = (TimeGetSet) message;
            if (gs.IsGet)
                BroadcastChannel.TimeGet(gs.IsExecute, gs.DateTime, Client.UniqueKey);
            else
                BroadcastChannel.TimeSet(gs.IsExecute, gs.DateTime, Client.UniqueKey);
            return null;
        }
    }
}