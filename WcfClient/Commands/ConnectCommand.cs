using System;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Clients;

namespace WcfClient.Commands
{
    public sealed class ConnectCommand<MT, T> : ClientCommand<MT, T>
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public ConnectCommand()
        {
        }

        public ConnectCommand(bool noNewBinding)
            : base(noNewBinding)
        {
        }

        protected override bool CanExecuteCommand(object par)
        {
            return Client.ClientStatus == ClientStatus.NotConnected
                   || Client.ClientStatus == ClientStatus.Refused;
        }

        protected override object ExecuteCommand(object par)
        {
            lock (Client.ConnectingLock)
            {
                if (CanExecuteCommand(par))
                {
                    Client.ClientStatus = ClientStatus.ConnectPending;
                    Client.CreateFactories();
                    try
                    {
                        CheckError(MainChannel.Connect(
                                       new Connection
                                           {
                                               ClientType = Client.Settings.ClientType,
                                               ClientUniqueKey = Client.UniqueKey,
                                               User = Client.User,
                                           }, out Client.Token));
                    }
                    catch(Exception ex)
                    {
                        // unsubscribe is impossible due to MainChannel fault 
                        Client.ClientStatus = ClientStatus.Refused;
                        Client.CloseFactories();
                        Client.OnConnectError(ex);
                        throw;
                    }
                    Client.ClientStatus = ClientStatus.Connected;
                }
            }
            return null;
        }
    }
}