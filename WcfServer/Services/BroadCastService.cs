using System;
using System.Collections.Generic;
using System.ServiceModel;
using ServiceModelEx;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Clients;

namespace WcfServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        MaxItemsInObjectGraph = 1000000,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class BroadCastService<T> : PublishService<T>, IBroadCastContract
        where T : class, IBroadCastContract
    {
        #region IBroadCastContract Members

        public void ConnectJoined(Connection connection, List<Connection> clients)
        {
            FireEvent(connection, clients);
        }

        public void ConnectDisconnected(string disconnectedUniqueKey, string initiatorUniqueKey,
                                        List<Connection> clients, string reason, bool kill)
        {
            FireEvent(disconnectedUniqueKey, initiatorUniqueKey, clients, reason, kill);
        }

        public void BroadcastMessage(string message, string clientUniqueKey)
        {
            FireEvent(message, clientUniqueKey);
        }

        public void ServerError(FaultException ex)
        {
            FireEvent(ex);
        }

        public void ServerRestarted(bool isFinish, string initiatorUniqueKey)
        {
            FireEvent(isFinish, initiatorUniqueKey);
        }

        public void TimeSet(bool toSet, DateTime date, string initiatorUniqueKey)
        {
            FireEvent(toSet, date, initiatorUniqueKey);
        }

        public void TimeGet(bool toSet, DateTime? date, string initiatorUniqueKey)
        {
            FireEvent(toSet, date, initiatorUniqueKey);
        }

        #endregion
    }
}