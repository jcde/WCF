using System.Collections;
using System.Windows.Controls;
using System.Windows.Threading;
using AppConfiguration.Wpf;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Clients;
using System;

namespace WcfClient.Commands
{
    public class DisconnectCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public DisconnectCommand()
        {
        }

        public DisconnectCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            if (par is Grid)
            {
                var i = 0;
                foreach (Connection e in ((WpfGrid)par).SelectedItems)
                    if (e.ClientUniqueKey != Client.UniqueKey)
                        i++;
                return base.CanExecuteCommand(par) && i > 0;
            }
            return base.CanExecuteCommand(par);
        }

        protected override object ExecuteCommand(object par)
        {
            return ExecuteCommand(par, false);
        }

        protected object ExecuteCommand(object par, bool toKill)
        {
            if (par is WpfGrid)
            {
                IList l = new ArrayList();
                SourceControl.Dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)delegate
                                                             {
                                                                 l = ((WpfGrid)par).SelectedItems;
                                                             });
                foreach (Connection c in l)
                {
                    if (c.ClientUniqueKey != Client.UniqueKey)
                    {
                        MainChannel.PuntUser(c.ClientUniqueKey, "Punted by other user", toKill);
                    }
                }
            }
            else if (par != null && par is string)
            {
                var s = (string)par;
                if (s == "All")
                    MainChannel.PuntUser(null, "Punted all", toKill);
                else
                    MainChannel.PuntUser(s, "Punted by reference", toKill);
            }
            else
            {
                Client.ClientStatus = ClientStatus.DisconnectPending;
                MainChannel.Disconnect();
            }
            return null;
        }
    }
}