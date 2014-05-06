using System;
using System.Collections.Generic;
using System.ServiceModel;
using WcfDomain.Contracts.Clients;
using WcfDomain.Contracts.Security;

namespace WcfDomain.Contracts
{
    /// <summary>
    /// describes events handled by client
    /// everyone connected receives these notifications 
    /// </summary>
    [ServiceContract(Namespace = Namespaces.Services)]
    public interface IBroadCastContract
    {
        #region Connections

        /// <summary>
        /// everyone connected including currently being connected receives notification 
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ConnectJoined(Connection connection, List<Connection> clients);

        /// <summary>
        /// everyone connected including currently being disconnected receives notification 
        /// </summary>
        /// <param name="disconnectedUniqueKey">if null, then all EXCEPT of initiatorUniqueKey were disconnected</param>
        /// <param name="kill">True = dear client, please Exit NOW!; False = dear client, you was disconnected and may exit</param>
        /// <param name="clients">list of clients AFTER disconnect</param>
        [OperationContract(IsOneWay = true)]
        void ConnectDisconnected(string disconnectedUniqueKey, string initiatorUniqueKey, List<Connection> clients,
                                 string reason, bool kill);

        #endregion

        /// <summary>
        /// everyone connected including initiator receives notification 
        /// </summary>
        [OperationPermission(SecureResource = SecureResource.MessageToAll)]
        [OperationContract(IsOneWay = true)]
        void BroadcastMessage(string message, string initiatorUniqueKey);

        /// <summary>
        /// notifies about unhandled error on server
        /// </summary>
        /// <param name="ex">NOT of type Exception!</param>
        [OperationPermission(SecureResource = SecureResource.MessageToAll)]
        [OperationContract(IsOneWay = true)]
        void ServerError(FaultException ex);

        /// <summary>
        /// Sometimes server has to be restarted, for example, 
        ///     in order to update server to new version 
        /// Such technical works on server must be invisible to clients as much as possible.
        /// </summary>
        /// <param name="isFinish">false - started, true, - finished</param>
        [OperationPermission(SecureResource = SecureResource.MessageToAll)]
        [OperationContract(IsOneWay = true)]
        void ServerRestarted(bool isFinish, string initiatorUniqueKey);

        /// <summary>Demands client to set time on its PC (toSet == true)
        /// or Reports about time setting (toSet == false)</summary>
        /// <param name="toSet">true = command to set, false = time was already set by initiatorUniqueKey</param>
        [OperationContract(IsOneWay = true)]
        void TimeSet(bool toSet, DateTime date, string initiatorUniqueKey);

        /// <summary>Demands client to notify about time on its PC (toSet == true)
        /// or Reports about time (toSet == false && date != null)</summary>
        /// <param name="toSet">true = command to set, false = time was already got by initiatorUniqueKey</param>
        [OperationContract(IsOneWay = true)]
        void TimeGet(bool toSet, DateTime? date, string initiatorUniqueKey);
    }
}