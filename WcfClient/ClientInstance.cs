using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using WcfDomain.Contracts;
using WcfClient.Commands;
using WcfClient.ErrorsHandling;
using WcfClient.Properties;
using ServiceModelEx;

namespace WcfClient
{
    //use in inherited classes to allow MainChannel calling during callback
    //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)] 
    public class ClientInstance<MT, T> : UniquePropertyNotifier, IDisposable
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        #region Contructors

        /// <summary>
        /// called from WPF designer
        /// </summary>
        public ClientInstance()
            : this("WPF")
        {
        }

        public ClientInstance(string appID)
            : this(appID, new CommandsManager<MT, T>())
        {
        }

        public ClientInstance(string server, int port, string appID)
            : this(server, port, appID, true)
        {
        }

        public ClientInstance(string server, int port, string appID, bool autoConnect)
            : this(server, port, appID, autoConnect, new CommandsManager<MT, T>())
        {
        }

        protected ClientInstance(string server, int port, string appID, bool autoConnect, CommandsManager<MT, T> cm)
            : this(appID, cm, false)
        {
            Settings.ClientSpecificMode = true;
            Settings.Server = server;
            Settings.Port = port;

            Settings.AutoConnect = autoConnect;

            CheckConnections(); // must be after server and port were set
        }

        protected ClientInstance(string appID, CommandsManager<MT, T> cm)
            : this(appID, cm, true)
        {
        }

        protected ClientInstance(string appID, CommandsManager<MT, T> cm, bool tryAutoConnect)
            : base(appID)
        {
            if (Settings.ClientSpecificMode)
                throw new ApplicationException(
                    "Current process possesses other client. Exclusive use is impossible.");
            commandsManager = cm;
            cm.Client = this;

            if (tryAutoConnect)
                CheckConnections();
        }

        private void CheckConnections()
        {
            connectionStateThread = new Thread(delegate()
                                               {
                                                   try
                                                   {
                                                       Thread.Sleep(1000);
                                                       while (true)
                                                       {
                                                           if (Settings.AutoConnect)
                                                           {
                                                               commandsManager.ExecuteIfCan(
                                                                   typeof (ConnectCommand<MT, T>));
                                                           }
                                                           CheckFaultState();
                                                           Thread.Sleep(3000);
                                                       }
                                                   }
                                                   catch (ThreadAbortException)
                                                   {
                                                   }
                                               })
                                        {
                                            Name = string.Format("WCF Client {0} - Connection Checker",
                                                            ApplicationID),
                                            CurrentCulture = new CultureInfo(AppConfiguration.Localization.Config.DefaultLanguage),
                                        };
            connectionStateThread.Start();
        }

        #endregion

        public readonly CommandsManager<MT, T> commandsManager;
        public string LastError;

        private readonly Dictionary<ComEnum, ClientCommand<MT, T>> comms
            = new Dictionary<ComEnum, ClientCommand<MT, T>>();

        private Thread connectionStateThread;

        /// <summary>
        /// is used for impersonating to other services, for example to chat client
        ///
        /// set during connection, therefore check on != Guid.Empty will show that client is connected
        /// !however Token may be not empty if client was punted
        /// </summary>
        /// <remark>
        /// public for tests
        /// </remark>
        public Guid Token = Guid.Empty;

        #region Settings

        private static readonly Dictionary<ClientInstance<MT, T>, Settings> _settingsClientSpecific
            = new Dictionary<ClientInstance<MT, T>, Settings>();

        public Settings Settings
        {
            get
            {
                lock (_settingsClientSpecific)
                    if (!_settingsClientSpecific.ContainsKey(this))
                    {
                        var s = new Settings();
                        s.InitValues();
                        s.PropertyChanged += Settings_Changed;
                        _settingsClientSpecific.Add(this, s);
                    }
                lock (_settingsClientSpecific)
                    return _settingsClientSpecific[this];
            }
        }

        private void Settings_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Server" || e.PropertyName == "Port")
            {
                CloseFactories();
                Refresh("ServerAddress");
            }
        }

        #endregion

        #region Status

        private ClientStatus _clientStatus = ClientStatus.NotConnected;
        internal object ConnectingLock=new object();

        public ClientStatus ClientStatus
        {
            get
            {
                //lock (this)
                    return _clientStatus;
            }
            internal set // setting allowed only through ClientCommand
            {
                //lock (this)
                    _clientStatus = value;
                Refresh("ClientStatus");
            }
        }

        #endregion

        #region Channells

        #region Main Channel

        public ICommunicationObject _mainChannel;

        /// <summary>
        /// lifetime - from connect to server till disconnect
        /// </summary>
        public DuplexChannelFactory<MT, T> MainChannelFactory;

        /// <summary>
        /// lifetime - from connection till disconnection
        /// may be null, factory not created
        /// </summary>
        public MT MainChannel
        {
            get
            {
                return Channels.Open(ref _mainChannel, MainChannelFactory,
                                     delegate
                                     {
                                         if (MainChannelFactory != null
                                             &&
                                             MainChannelFactory.Endpoint.Behaviors.Find<ErrorClientBehavior>() ==
                                             null)
                                         {
                                             var handler = new ClientErrorHandler<MT, T>(this, MainChannelFactory);
                                             MainChannelFactory.Opening
                                                 += (delegate
                                                         {
                                                             if (MainChannelFactory.Endpoint.Behaviors
                                                                     .Find<ErrorClientBehavior>() == null)
                                                                 MainChannelFactory.Endpoint.Behaviors.Add(
                                                                     new ErrorClientBehavior(handler));
                                                         });
                                         }
                                     });
            }
        }

        public string ServerAddress
        {
            get { return GetServerAddress(Namespaces.MainServicePath); }
        }

        #endregion

        #region Broadcasts

        internal BroadcastChannels<T> BroadcastChannels;

        /// <summary>
        /// should be inited before first CreateFactories call
        /// as a rule before user clicks Connect button
        /// </summary>
        public T BroadcastInstance;

        #endregion

        protected string GetServerAddress(string path)
        {
            return string.Format("net.tcp://{0}:{1}/{2}",
                                 Settings.Server, Settings.Port,
                                 path);
        }


        /// <summary>
        /// is called more often than real channel closing
        /// </summary>
        public virtual void BeforeCloseChannel()
        {
        }

        /// <summary>
        /// is fired when connection went down rudely
        /// </summary>
        public virtual void AfterChannelFault()
        {
        }

        /// <summary>
        /// unfortunately it is fired even if real connection is not established,
        /// but only when state is changed
        /// </summary>
        public virtual void AfterMainChannelOpened()
        {
        }

        public virtual void AfterCreateChannels(NetTcpBinding binding)
        {
        }

        internal void CreateFactories()
        {
            CloseFactories();

            BroadcastChannels = new BroadcastChannels<T>(Settings.Server, Settings.Port,
                                                         BroadcastInstance);
            BroadcastChannels.OpenChannelAction =
                delegate
                    {
                        KeyedByTypeCollection<IEndpointBehavior> behaviors =
                            BroadcastChannels.ChannelBroadcastFactory.Endpoint.Behaviors;
                        if (behaviors.Find<ErrorClientBehavior>() == null)
                        {
                            var handler = new ClientErrorHandler<MT, T>(this,
                                                                        BroadcastChannels.ChannelBroadcastFactory);
                            BroadcastChannels.ChannelBroadcastFactory
                                .Opening += (delegate
                                                 {
                                                     behaviors.Add(new ErrorClientBehavior(handler));
                                                 });
                        }
                    };

            NetTcpBinding binding = Channels.GetBinding();

            MainChannelFactory = new DuplexChannelFactory<MT, T>
                (BroadcastChannels.Callback, binding, new EndpointAddress(ServerAddress));
            MainChannelFactory.Opened += delegate
                                             {
                                                 AfterMainChannelOpened();
                                             };
            AfterCreateChannels(binding);
        }

        public void CloseFactories()
        {
            if (ClientStatus == ClientStatus.Connected)
                new DisconnectCommand<MT, T>(true).ExecuteSafe(this, null);
            else
                ClientStatus = ClientStatus.NotConnected;

            BeforeCloseChannel();

            Channels.CloseChannel(ref _mainChannel);
            Channels.CloseFactory(ref MainChannelFactory);

            if (BroadcastChannels != null)
            {
                BroadcastChannels.Dispose();
                BroadcastChannels = null;
            }
        }

        #endregion

        #region Broadcasted Execution

        protected int calls;
        protected int? previousCalls;
        public List<Exception> serverErrors = new List<Exception>();

        /// <summary>
        /// broadcasted event must increase value of 'calls'
        /// </summary>
        public void WaitBroadcasting()
        {
            if (previousCalls == null)
                throw new ApplicationException("wait not started");
            Wait(() => calls == previousCalls);
        }

        public virtual void Clear()
        {
            calls = 0;
            serverErrors.Clear();
        }

        #endregion

        #region Waiting

        /// <param name="func">condition of waiting</param>
        public static void Wait(Func<bool> func)
        {
            Wait(func, 10);
        }

        public static void Wait(Func<bool> func, double seconds)
        {
            if (func())
            {
                int waited = 0;
                do
                {
                    Thread.Sleep(300);
                    waited++;
                } while (func() && waited < seconds/0.3);
                if (func())
                    throw new FinishWaitTimeoutException();
            }
        }

        public void StartWait()
        {
            previousCalls = calls;
        }

        /// <summary>
        /// waits until broadcasting finish
        /// if you don't want to wait then call commandsManager.Execute
        /// </summary>
        public object ExecuteAsync(Type type)
        {
            return ExecuteAsync(type, null);
        }

        public object ExecuteAsync(ComEnum en)
        {
            return ExecuteAsync(commandsManager.CommandType(en));
        }

        public object ExecuteAsync(ComEnum en, object key)
        {
            return ExecuteAsync(commandsManager.CommandType(en), key);
        }

        public object ExecuteAsync(Type type, object key)
        {
            StartWait();
            object r = commandsManager.Execute(type, key);
            WaitBroadcasting();
            return r;
        }

        public object ExecuteAsync(ClientCommand<MT, T> com, string key)
        {
            StartWait();
            object r = com.ExecuteSafe(this, key);
            WaitBroadcasting();
            return r;
        }

        public void WaitConnected()
        {
            WaitConnected(10);
        }

        public void WaitConnected(double seconds)
        {
            CheckFaultState();
            Wait(() => ClientStatus != ClientStatus.Connected, seconds);
        }

        private int pinged;
        private void CheckFaultState()
        {
            lock (ConnectingLock)
                if (_mainChannel != null)
                {
                    if (_mainChannel.State == CommunicationState.Faulted)
                    {
                        CloseFactories();
                        AfterChannelFault();
                    }
                    else if (pinged++ > 100) // every 300 seconds
                    {
                        try
                        {
                            object r;
                            ((MT) _mainChannel).GeneralMessage(GeneralCommandType.Ping, null, out r);
                        }
                        catch (Exception)
                        {
                            CloseFactories();
                            AfterChannelFault();
                        }
                        pinged = 0;
                    }
                }
                else
                {
                    CloseFactories();
                }
        }

        #endregion

        public ClientCommand<MT, T> this[ComEnum en]
        {
            get
            {
                if (!comms.ContainsKey(en))
                    comms.Add(en, commandsManager.Command(en, false));
                return comms[en];
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (connectionStateThread != null
                && connectionStateThread.ThreadState != ThreadState.AbortRequested)
            {
                connectionStateThread.Abort();
                connectionStateThread = null;
            }
            CloseFactories();
            Settings.Save();
        }

        #endregion

        public virtual void OnConnectError(Exception ex)
        {}
    }
}