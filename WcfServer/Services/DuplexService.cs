using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using AppConfiguration;
using AppConfiguration.Localization;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using ServiceModelEx;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Clients;
using WcfDomain.Threads;
using WcfServer.Performance;
using WcfServer.Properties;
using Settings = WcfServer.Properties.Settings;

namespace WcfServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        Namespace = Namespaces.Services,
        MaxItemsInObjectGraph = 1000000,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class DuplexService<T> : SubscriptionManager<T>, IContract, IDisposable
        where T : class, IBroadCastContract
    {
        public readonly int ServerListenPort = Settings.Default.ServerListenPort;
        public readonly int ServerMexPort = Settings.Default.ServerMexPort;
        private readonly Thread checkConnections;
        internal ServiceState ServiceState = ServiceState.Starting;

        #region Connections

        protected readonly List<Connection> Clients = new List<Connection>();

        /// <summary>
        /// secret information that can not be included into public Connection
        /// </summary>
        internal readonly Dictionary<Guid, Connection> Tokens = new Dictionary<Guid, Connection>();

        protected UniquePropertyNotifier Server
        {
            get { return new UniquePropertyNotifier("System"); }
        }

        /// <summary>
        /// may be null during reconnection 
        ///     or when operation is called by server, from ServiceHost 
        /// </summary>
        protected Connection Sender
        {
            get
            {
                return OperationContext.Current != null
                           ? GetClient(OperationContext.Current.Channel)
                           : null;
            }
        }

        protected User SenderUser
        {
            get { return Sender != null ? Sender.User : null; }
        }

        public List<Connection> RequestUsers()
        {
            return (List<Connection>) ComplexMonitor.CopyFrom(Clients);
        }

        public Connection GetClient(string initiatorUniqueKey, bool withDelphi)
        {
            return (from e in Clients
                    where e.ClientUniqueKey == initiatorUniqueKey
                          && (e.ClientType != ClientType.SendAndReceiver_WithoutChat
                              || withDelphi)
                    select e).SingleOrDefault();
        }

        internal Connection GetClient(IContextChannel channel)
        {
            return (from e in Clients
                    where e.ServerChannel == channel
                    select e).SingleOrDefault();
        }

        public List<Connection> GetClientsNotDelphi(User user)
        {
            return GetClients(new[] {user}, false);
        }

        public List<Connection> GetClients(IEnumerable<User> users, bool withDelphi)
        {
            return (from c in Clients
                    join u in users on c.User equals u
                    where c.ClientType != ClientType.SendAndReceiver_WithoutChat
                          || withDelphi
                    select c).ToList();
        }

        private List<Connection> GetClients(string computerName)
        {
            return (from c in Clients
                    where string.IsNullOrEmpty(computerName)
                          || c.User.ComputerName.ToUpper() == computerName.ToUpper()
                    select c).ToList();
        }

        private void CheckConnections()
        {
            foreach (Connection c in new List<Connection>(Clients))
                if (c.LastReceiveDate != null
                    && (DateTime.UtcNow - (DateTime) c.LastReceiveDate).Minutes
                    >= Settings.Default.DropConnectionTimeoutMinutes)
                    PuntUser(c.ClientUniqueKey,
                             string.Format("There was no traffic with client during {0} minutes",
                                           Settings.Default.DropConnectionTimeoutMinutes), false);
        }

        #region Connect

        protected virtual bool HardReconnect
        {
            get { return true; }
        }

        public string Connect(Connection connection, out Guid token)
        {
            token = Guid.Empty;

            if (ServiceState != ServiceState.Run)
            {
                return Resources.ServiceNotRun;
            }

            string r = OnConnecting(connection);
            if (r != null)
                return r;

            //please don't lock (connection)
            {
                if (Clients.Contains(connection))
                {
                    if (HardReconnect)
                    {
                        Logger.Write("Re-Connection by punting: " + connection.ClientUniqueKey);
                        PuntUser(connection, "Re-Connection");
                    }
                    else
                    {
                        Logger.Write("Re-Connection attempt: " + connection.ClientUniqueKey);
                        Connection client = GetClient(connection.ClientUniqueKey, false);
                        if (client != null && OperationContext.Current != null)
                            client.ServerChannel = OperationContext.Current.Channel;
                        ConnectNotify(connection, true);
                    }
                    return null;
                }

                if (IsClientTypeAllowed(connection.ClientType))
                {
                    if (!RoomForClientType(connection.ClientType))
                    {
                        string fMsg = "You have reached the limit of " + LimitForClientType(connection.ClientType) +
                                      " for clients type " + @Enum.GetName(typeof (ClientType), connection.ClientType) +
                                      ".";
                        Logger.Write(fMsg + Environment.NewLine + Environment.NewLine + connection.ClientUniqueKey);
                        return fMsg;
                    }
                }
                else
                {
                    string fMsg = "Client type of " + @Enum.GetName(typeof (ClientType), connection.ClientType) +
                                  " is not allowed by the current server settings.";
                    Logger.Write(fMsg + Environment.NewLine + Environment.NewLine + connection.ClientUniqueKey);
                    return fMsg;
                }

                switch (connection.ClientType)
                {
                    case ClientType.Receiver:
                    case ClientType.SendAndReceiver:
                    case ClientType.SendAndReceiver_WithoutChat:
                    case ClientType.Any:
                        Subscribe(null);
                        break;
                }
                connection.ConnectTime = DateTime.UtcNow;
                connection.ServerChannel = OperationContext.Current.Channel;
                MessageProperties prop = OperationContext.Current.IncomingMessageProperties;
                connection.IP = ((RemoteEndpointMessageProperty) prop[RemoteEndpointMessageProperty.Name]).Address;

                Clients.Add(connection);
                token = Guid.NewGuid();
                Tokens.Add(token, connection);
                // if the client disconnects w/o telling us
                OperationContext.Current.Channel.Faulted +=
                    delegate
                        {
                            Thread.CurrentThread.CurrentCulture =
                                new CultureInfo(Config.DefaultLanguage);
                            Logger.Write(string.Format("{0} connection faulted, therefore it is being disconnected",
                                                       connection));
                            PuntUser(connection, "client application faulted", false);
                        };
                OperationContext.Current.Channel.Closed +=
                    delegate
                        {
                            if (Clients.Contains(connection))
                            {
                                Thread.CurrentThread.CurrentCulture =
                                    new CultureInfo(Config.DefaultLanguage);
                                Logger.Write(string.Format("{0} connection closed, therefore it is being disconnected",
                                                           connection));
                                PuntUser(connection, "client application closed", false);
                            }
                        };

                Logger.Write(string.Format("Client connected [{0}]", connection.ClientUniqueKey));

                ConnectNotify(connection, false);
                PerfCounters.Count(PerfCounters.Connections, Clients.Count);
                return null;
            }
        }

        private void ConnectNotify(Connection connection, bool reconnected)
        {
            if (connection.ClientType == ClientType.Sender)
                return;
            if (OnConnectNotifyBefore(connection, reconnected))
            {
                List<Connection> users = RequestUsers();
                Clients.ForEach(
                    c =>
                        {
                            try
                            {
                                c.BroadcastChannel<T>().ConnectJoined(connection, users);
                            }
                            catch (SystemException ex)
                            {
                                Logger.Write(ex);
                            }
                        });
                OnConnectNotifyAfter(connection);
            }
        }

        /// <returns>not null - bad, not connected, error description</returns>
        protected virtual string OnConnecting(Connection c)
        {
            return null;
        }

        /// <summary>
        /// called before broadcasting to Sender
        /// </summary>
        /// <returns>true - if all good</returns>
        protected virtual bool OnConnectNotifyBefore(Connection c, bool reconnected)
        {
            return true;
        }

        /// <summary>
        /// after notification to all users
        /// </summary>
        protected virtual void OnConnectNotifyAfter(Connection c)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.DefaultLanguage);
        }

        private static bool IsClientTypeAllowed(ClientType clientType)
        {
            return LimitForClientType(clientType) > 0;
        }

        /// <returns>maximum clients allowed, does NOT return -1</returns>
        private static int LimitForClientType(ClientType clientType)
        {
            int limit = 0;
            switch (clientType)
            {
                case ClientType.SendAndReceiver:
                    limit = Settings.Default.AllowClientType_SenderReceiver;
                    break;
                case ClientType.Sender:
                    limit = Settings.Default.AllowClientType_Sender;
                    break;
                case ClientType.Receiver:
                    limit = Settings.Default.AllowClientType_Receiver;
                    break;
                case ClientType.SendAndReceiver_WithoutChat:
                    limit = -1;
                    break;
            }
            return limit == -1 || limit > Settings.Default.AllowClients
                       ? Settings.Default.AllowClients
                       : limit;
        }

        private bool RoomForClientType(ClientType clientType)
        {
            int allowedForType = LimitForClientType(clientType);
            int thereare = (from e in Clients
                            where e.ClientType == clientType
                            select e).Count();
            return thereare < allowedForType && thereare < Settings.Default.AllowClients;
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            PuntUser(Sender, "Own will", false);
        }

        public void PuntUser(string uniqueKey, string reason, bool kill)
        {
            if (uniqueKey == null)
                DisconnectAllExceptSender(reason, kill);
            else
            {
                Connection c = Clients.Find(e => e.ClientUniqueKey == uniqueKey);
                if (c != null)
                {
                    PuntUser(c, reason, kill);
                }
            }
        }

        /// <summary>
        /// overriding methods MUST check whether channel is opened
        /// </summary>
        protected virtual void OnDisconnectedBefore(Connection c)
        {
        }

        /// <summary>
        /// it is clear that c cannot use any channels
        /// </summary>
        protected virtual void OnDisconnectedAfter(Connection c)
        {
        }

        protected internal void PuntUser(Connection c, string reason)
        {
            PuntUser(c, reason, false);
        }

        internal void PuntUser(Connection c, string reason, bool kill)
        {
            if (Clients.Contains(c)) // because during disconnection repeatable Disconnect may be called
            {
                string initiatorUniqueKey = Sender != null ? Sender.ClientUniqueKey : c.ClientUniqueKey;

                try
                {
                    OnDisconnectedBefore(c);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }

                //don't zero c.ServerChannel = null; 
                var clientsTill = new List<Connection>(Clients);
                Clients.Remove(c);
                var oldTokens = new Dictionary<Guid, Connection>(Tokens);
                (from pair in oldTokens
                 where pair.Value == c
                 select pair.Key).ForEach(a => Tokens.Remove(a));

                DisconnectNotify(c, initiatorUniqueKey, clientsTill, RequestUsers(), reason, kill);
                try
                {
                    OnDisconnectedAfter(c);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
        }

        /// <summary>
        /// at this moment channel is closed
        /// </summary>
        /// <param name="clients">must be safe-threaded</param>
        private void DisconnectNotify(Connection disconnected, string initiatorUniqueKey,
                                      IEnumerable<Connection> clientsTill, List<Connection> clients, string reason,
                                      bool kill)
        {
            if (disconnected != null)
            {
                DisconnectNotifyBroadcast(disconnected.ClientUniqueKey, disconnected, initiatorUniqueKey,
                                          clients, reason, kill);
            }
            else
            {
                foreach (Connection c in clientsTill)
                {
                    if (initiatorUniqueKey == null || c.ClientUniqueKey != initiatorUniqueKey)
                        DisconnectNotifyBroadcast(null, c, initiatorUniqueKey, clients, reason, kill);
                }
            }

            PerfCounters.Count(PerfCounters.Connections, clients == null ? 0 : clients.Count);
        }

        private void DisconnectNotifyBroadcast(string disconnectedKey, Connection disconnected,
                                               string initiatorUniqueKey, List<Connection> clients, string reason,
                                               bool kill)
        {
            BroadcastsChannel.ConnectDisconnected(disconnectedKey, initiatorUniqueKey,
                                                  clients, reason, kill);

            var cC = disconnected.ServerChannel as ICommunicationObject;
            if (cC != null && cC.State == CommunicationState.Opened)
            {
                // the following cC.Close(); will interrupt overall broadcasting 
                //      therefore disconnected should be notified personally
                try
                {
                    disconnected.BroadcastChannel<T>().ConnectDisconnected(disconnectedKey, initiatorUniqueKey,
                                                                           clients, reason, kill);
                }
                catch (SystemException)
                {
                }

                // closing is vital because without closing client will be only notified 
                //      but it can continue to work 
                Channels.CloseChannel(ref cC);
            }
            Unsubscribe(null, (T) disconnected.ServerChannel);
        }

        /// <summary>
        /// thread-UNsafe when called in not Reentrant thread (for example, while WinService is being stopped)
        /// </summary>
        public void DisconnectAllExceptSender(string reason, bool kill)
        {
            var oldClients = new List<Connection>(Clients);

            if (Sender != null)
            {
                var oldTokens = new Dictionary<Guid, Connection>(Tokens);
                Guid token = GetToken(oldTokens, Sender);
                Tokens.Clear(); // blocks dependent services like ChatMessagesService
                if (token != Guid.Empty)
                    Tokens.Add(token, Sender);
                foreach (Connection c in oldClients)
                    if (c != Sender)
                        Clients.Remove(c);
            }
            else
            {
                Tokens.Clear(); // blocks dependent services like ChatMessagesService
                Clients.Clear();
                Clear();
            }
            foreach (Connection c in oldClients)
            {
                try
                {
                    OnDisconnectedBefore(c);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            DisconnectNotify(null, Sender != null ? Sender.ClientUniqueKey : null,
                             oldClients, RequestUsers(), reason, kill);
            foreach (Connection c in oldClients)
            {
                try
                {
                    OnDisconnectedAfter(c);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
        }

        internal static Guid GetToken(IEnumerable<KeyValuePair<Guid, Connection>> oldTokens, Connection c)
        {
            return (from e in oldTokens
                    where e.Value == c
                    select e.Key).SingleOrDefault();
        }

        #endregion

        #endregion

        #region Broadcasts

        private readonly BroadcastChannels<T> broadcasts;

        /// <summary>
        /// sends to ALL Clients
        /// </summary>
        public T BroadcastsChannel
        {
            get { return broadcasts.Channel; }
        }

        public T SenderBroadcastsChannel
        {
            get
            {
                if (OperationContext.Current != null)
                {
                    try
                    {
                        return OperationContext.Current.GetCallbackChannel<T>();
                    }
                    catch (InvalidCastException)
                    {
                    }
                }
                return null;
            }
        }

        public List<Connection> Broadcast(User user)
        {
            return GetClients(new[] {user}, true);
        }

        #endregion

        public DuplexService()
            : this(Settings.Default.ServerListenPort)
        {
        }

        public DuplexService(int port)
        {
            broadcasts = new BroadcastChannels<T>("localhost",
                                                  port,
                                                  null);
            PerfCounters = new PerfCounters(Settings.Default.ServerListenPort.ToString());
            checkConnections = new Thread(
                delegate()
                    {
                        try
                        {
                            while (true)
                            {
                                Thread.Sleep(new TimeSpan(0, Settings.Default.DropConnectionCheckMinutes, 0));
                                CheckConnections();
                            }
                        }
                        catch (ThreadAbortException)
                        {
                        }
                    })
                                   {
                                       Name = string.Format("WCF {0} - Check Connections",
                                                            GetType().Name),
                                       CurrentCulture =
                                           new CultureInfo(Config.DefaultLanguage),
                                   };
            checkConnections.Start();
            ServiceState = ServiceState.Run;
        }

        public DuplexService(int port, int mexPort)
            : this(port)
        {
            ServerListenPort = port;
            ServerMexPort = mexPort;
        }

        public PerfCounters PerfCounters { get; private set; }

        #region Settings

        public void SaveSetting(string configFile, string settingName, object value, bool isConnection)
        {
            ConfigurationWriter.WriteApplicationSetting(configFile, settingName, value, isConnection);
        }

        public List<ConfigurationSetting> ServerSettings(string settingName, object value, ref object message)
        {
            message = Resources.RestartedSettings;
            if (settingName != null)
            {
                ConfigurationWriter.WriteApplicationSetting(settingName, value);
                return ConfigurationSetting.FindAllSettings();
            }
            return ConfigurationSetting.FindAllSettings(true);
        }

        public string RequestSetting(string settingName, bool defaultValue, out object result)
        {
            result = null;
            SettingsProperty property = Settings.Default.Properties[settingName];
            if (property == null)
            {
                //very slow 2 seconds and more
                ConfigurationSetting set = ConfigurationSetting.FindAllSettings(true)
                    .Find(s => s.PropertyName == settingName
                               || string.Format("{0}.{1}",
                                                s.Property.DeclaringType.FullName,
                                                s.PropertyName) == settingName);
                if (set != null)
                {
                    result = set.Value;
                    return null;
                }
                return "Wrong Setting name";
            }
            result = defaultValue ? property.DefaultValue : Settings.Default[settingName];
            return null;
        }

        #endregion

        #region IContract Members

        public string GeneralMessage(GeneralCommandType messageType, string data, out object result)
        {
            result = null;
            switch (messageType)
            {
                case GeneralCommandType.TimeRequest:
                    DateTime setDate;
                    result = TimeRequest(null, null,
                                         DateTime.TryParse(data, out setDate) ? setDate : (DateTime?) null);
                    break;
                case GeneralCommandType.Log:
                    string s = data;
                    if (Sender != null)
                        s += string.Format(" Client: {0}({1})",
                                           Sender.ClientUniqueKey, Sender.IP);
                    Logger.Write(s, "Server Log");
                    break;
                case GeneralCommandType.Ping:
                    if (Sender != null)
                        try
                        {
                            SenderBroadcastsChannel.BroadcastMessage("Pinged", Sender.ClientUniqueKey);
                        }
                        catch
                        {
                        }
                    break;
                case GeneralCommandType.SettingGetRequest:
                    return RequestSetting(data, false, out result);
                case GeneralCommandType.SettingDefaultRequest:
                    return RequestSetting(data, true, out result);
                case GeneralCommandType.VersionInfoRequest:
                    result = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    break;
                case GeneralCommandType.RestartServer:
                    return Restart();
            }
            return null;
        }

        public string Restart()
        {
            // since this method can be called from GeneralMessage(...)
            ClaimsPolicy.ServerMethodCheck(typeof (IContract).GetMethod(MethodBase.GetCurrentMethod().Name));

            //1.	Server broadcasts to clients about start of technical restart
            BroadcastsChannel.ServerRestarted(false, Sender.ClientUniqueKey);

            //2.	Clients have to pause all their network activities
            //3.	Server blocks all incoming requests from clients, but server finishes all current activities – it does not abort them.
            //4.	Server stores list of clients connected (sClients) and other internal data like ChatRooms, RecordLocks etc
            //5.	Windows service is stopped without clients disconnecting (but their channels are broken)
            //6.	New binaries are copied
            //7.	Windows service is started
            //8.	New Server allows incoming requests for connection only from previously connected clients (sClients)
            //9.	Server waits till 10 seconds until all sClients will be reconnected
            //10.	Server restores previous internal data for clients that were reconnected successfully.
            //11.	Server allows all incoming requests from clients. 

            //12.	Server broadcasts to clients about end of technical restart
            BroadcastsChannel.ServerRestarted(true, Sender.ClientUniqueKey);
            return null;
        }

        public DateTime? TimeRequest(string workstationApplied, string workstationReturn, DateTime? setDate)
        {
            DateTime? actualTime = null;
            if (setDate == null)
            {
                if (string.IsNullOrEmpty(workstationApplied)
                    || workstationApplied.ToUpper() == Environment.MachineName.ToUpper())
                {
                    actualTime = DateTime.Now;
                    GetClients(workstationReturn)
                        .ForEach(a =>
                                     {
                                         try
                                         {
                                             a.BroadcastChannel<T>().TimeGet(false, actualTime, Server.UniqueKey);
                                         }
                                         catch (SystemException)
                                         {
                                             PuntUser(a, "bad connection");
                                         }
                                     });
                }
                else
                {
                    // time may be got only from ONE workstation
                    // this is server
                    GetClients(workstationApplied)
                        .ForEach(a =>
                                     {
                                         try
                                         {
                                             a.BroadcastChannel<T>().TimeGet(true, null, Sender.ClientUniqueKey);
                                         }
                                         catch (SystemException)
                                         {
                                             PuntUser(a, "bad connection");
                                         }
                                     });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(workstationApplied)
                    || workstationApplied.ToUpper() == Environment.MachineName.ToUpper())
                {
                    if (DateUtils.SetLocalTime((DateTime) setDate))
                    {
                        actualTime = setDate;
                        GetClients(workstationReturn)
                            .ForEach(a =>
                                         {
                                             try
                                             {
                                                 a.BroadcastChannel<T>().TimeSet(false, (DateTime) actualTime,
                                                                                 Server.UniqueKey);
                                             }
                                             catch (SystemException)
                                             {
                                                 PuntUser(a, "bad connection");
                                             }
                                         });
                    }
                    else
                    {
                        throw new ApplicationException("unsuccessful time setting on server");
                    }
                }

                var l = new List<Connection>();
                if (string.IsNullOrEmpty(workstationApplied))
                {
                    l = RequestUsers();
                    BroadcastsChannel.TimeSet(true, (DateTime) setDate, Sender.ClientUniqueKey);
                }
                else if (workstationApplied.ToUpper() != Environment.MachineName.ToUpper())
                {
                    l = GetClients(workstationApplied);
                }
                l.ForEach(a =>
                              {
                                  try
                                  {
                                      a.BroadcastChannel<T>().TimeSet(true, (DateTime) setDate, Sender.ClientUniqueKey);
                                  }
                                  catch (SystemException)
                                  {
                                      PuntUser(a, "bad connection");
                                  }
                              });
            }

            return actualTime;
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            PerfCounters.Dispose();
            checkConnections.Abort();
            ServiceState = ServiceState.Closed;
        }

        #endregion

        protected void GeneralMessage(GeneralCommandType messageType, string data)
        {
            object r;
            GeneralMessage(messageType, data, out r);
        }

        /// <summary>
        /// light variables are cleared
        /// that do not influence on Connections
        /// and preserve defaults
        /// </summary>
        public virtual void Clear()
        {
            PerfCounters.Clear();
        }
    }
}