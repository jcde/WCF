using System;
using System.Collections.Generic;
using System.ServiceModel;
using AppConfiguration;
using ServiceModelEx;
using WcfDomain.Contracts.Clients;
using WcfDomain.Contracts.Security;

namespace WcfDomain.Contracts
{
    /// <summary>
    /// server specification
    /// </summary>
    [ServiceContract(Namespace = Namespaces.Services,
        CallbackContract = typeof (IBroadCastContract))]
    public interface IContract : ISubscriptionService
    {
        #region Connections

        /// <summary>
        /// registers client as connected to server
        /// after connecting as a rule subscribtion to events is fulfilled
        /// </summary>
        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationContract]
        string Connect(Connection connection, out Guid token);

        /// <summary>
        /// gets out from the system
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Disconnect();

        /// <summary>
        /// punt other user
        /// </summary>
        /// <param name="uniqueKey">if null, then all clients except current will be punted</param>
        [OperationPermission(SecureResource = SecureResource.UserManagement)]
        [OperationContract(IsOneWay = true)]
        void PuntUser(string uniqueKey, string reason, bool kill);

        [OperationContract]
        List<Connection> RequestUsers();

        #endregion

        /// <summary>
        /// does not Reload Settings.Default. you have to do this manually
        /// </summary>
        [OperationContract]
        void SaveSetting(string configFile, string settingName, object value, bool isConnection);

        /// <summary>
        /// retrieves all settings in AppDomain
        /// and allows to set one setting's value
        /// </summary>
        /// <param name="settingName">used if not null</param>
        /// <param name="value"></param>
        /// <param name="message">returns settings' description</param>
        [OperationContract]
        List<ConfigurationSetting> ServerSettings(string settingName, object value, ref object message);

        /// <summary>
        /// for some compatibility with VB version of socket server
        /// </summary>
        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationContract]
        string GeneralMessage(GeneralCommandType messageType, string data, out object result);

        /// <returns>null - success, 
        /// not null - failure description</returns>
        [OperationPermission(SecureResource = SecureResource.ServerStartStop)]
        [OperationContract]
        string Restart();

        /// <summary>
        /// Gets time or modify time
        /// </summary>
        /// <param name="workstationApplied">null or empty string 
        /// = ALL Clients (for time-setting) 
        /// and = SERVER (for time-getting)</param>
        /// <param name="workstationReturn">workstation that should be notified about time GETTING
        /// (time-setting is notified to ALL clients)
        /// null or empty string = ALL Clients</param>
        /// <param name="setDate">null = only Get time</param>
        /// <returns>actual time on workstationApplied in its local timezone.
        /// if  workstationApplied is not server then returns null and actual time should broadcasted by IBroadCastContract.TimeSet</returns>
        [OperationContract]
        DateTime? TimeRequest(string workstationApplied, string workstationReturn, DateTime? setDate);
    }
}