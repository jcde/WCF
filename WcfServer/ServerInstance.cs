using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Policy;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using AppConfiguration;
using AppConfiguration.Localization;
using NLog;
using WcfDomain.Contracts;
using WcfServer.Services;
using Settings = WcfServer.Properties.Settings;

namespace WcfServer
{
    public class ServerInstance<MT, T> : IDisposable
        where MT : class, IContract
        where T : class, IBroadCastContract
    {
        public bool StartedWithoutErrors;
        protected DuplexService<T> _service;

        private ServiceHost serviceHost;
        private ServiceHost serviceHostBroadCast;

        public ServerInstance(DuplexService<T> service) :
            this(service, new BroadCastService<IBroadCastContract>())
        {
        }

        public ServerInstance(DuplexService<T> service, IBroadCastContract broadcastService)
        {
            Thread.CurrentThread.CurrentCulture =
                new CultureInfo(Config.DefaultLanguage);

            Close();

            try
            {
                _service = service;
                serviceHost = new ServiceHost(_service,
                                              new Uri(string.Format("http://localhost:{0}/{1}",
                                                                    _service.ServerMexPort,
                                                                    Namespaces.MainServicePath)));
                AddCommonEndpoints(typeof (MT), serviceHost,
                                   Namespaces.MainServicePath, true, true, _service.ServerListenPort)
                    .Behaviors.Add(new MessageInspector<T>());
                serviceHost.Open();

                BeforeBroadCastOpen();

                serviceHostBroadCast = new ServiceHost(broadcastService,
                                                       new Uri(string.Format("http://localhost:{0}/{1}",
                                                                             _service.ServerMexPort,
                                                                             Namespaces.BroadcastServicePath)));
                AddCommonEndpoints(typeof (T), serviceHostBroadCast,
                                   Namespaces.BroadcastServicePath, true, false, _service.ServerListenPort)
                    .Behaviors.Add(new MessageInspector<T> {BroadcastPerf = _service.PerfCounters});
                // BroadCastService should be opened last, 
                //    therefore it can be used during other service construction
                serviceHostBroadCast.Open();
                StartedWithoutErrors = true;
                Log.Default.Info("Wcf server at port {0} started successfully", _service.ServerListenPort);
            }
            catch (Exception commProblem)
            {
                Log.Default.Info(commProblem.Message, "ErrorServerOpen");
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            Close();
        }

        #endregion

        public virtual void BeforeBroadCastOpen()
        {
        }

        public virtual void BeforeServerClose()
        {
        }

        public virtual void AfterServerClose()
        {
        }

        protected virtual ServiceEndpoint AddCommonEndpoints(Type type, ServiceHost sh, string path, bool withMex,
                                                             bool authPolicy, int serverListenPort)
        {
            NetTcpBinding binding = Channels.GetBinding(Settings.Default.AllowClients);

            var address = new Uri(string.Format("net.tcp://localhost:{0}/{1}",
                                                serverListenPort,
                                                path));
            ServiceEndpoint endpoint = sh.AddServiceEndpoint(type, binding, address);

            ServiceMetadataBehavior smb = sh.Description.Behaviors.Find<ServiceMetadataBehavior>()
                                          ?? new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            smb.HttpGetEnabled = true;
            if (sh.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                sh.Description.Behaviors.Add(smb);

            ServiceThrottlingBehavior throttle = sh.Description.Behaviors.Find<ServiceThrottlingBehavior>()
                                                 ?? new ServiceThrottlingBehavior();
            throttle.MaxConcurrentSessions = Settings.Default.AllowClients;
            if (sh.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
                sh.Description.Behaviors.Add(throttle);

            if (authPolicy && Settings.Default.ClaimsSecurityEnabled)
            {
                ServiceAuthorizationBehavior auth = sh.Description.Behaviors.Find<ServiceAuthorizationBehavior>()
                                                    ?? new ServiceAuthorizationBehavior();
                auth.ExternalAuthorizationPolicies = new ReadOnlyCollection<IAuthorizationPolicy>
                    (new[] {new ClaimsPolicy()});
                if (sh.Description.Behaviors.Find<ServiceAuthorizationBehavior>() == null)
                    sh.Description.Behaviors.Add(auth);
            }

            if (withMex)
                sh.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "mex");

            sh.UnknownMessageReceived += service_UnknownMessageReceived;

#if DEBUG
            ((ServiceDebugBehavior) sh.Description.Behaviors[typeof (ServiceDebugBehavior)])
                .IncludeExceptionDetailInFaults = true;
#endif
            return endpoint;
        }

        private static void service_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            Log.Default.Info(e.Message.Headers[0] + "ErrorServerUnknown");
        }

        private void Close()
        {
            StartedWithoutErrors = false;
            BeforeServerClose();

            if (serviceHost != null
                && serviceHost.State == CommunicationState.Opened)
            {
                if (_service != null)
                {
                    _service.ServiceState = ServiceState.Closing;
                    _service.DisconnectAllExceptSender("Server stopped.", false);
                    _service.Dispose();
                    _service.ServiceState = ServiceState.Closed;
                }
                serviceHost.Close();
            }
            serviceHost = null;

            if (serviceHostBroadCast != null
                && serviceHostBroadCast.State == CommunicationState.Opened)
            {
                serviceHostBroadCast.Close();
            }
            serviceHostBroadCast = null;

            AfterServerClose();
        }
    }
}