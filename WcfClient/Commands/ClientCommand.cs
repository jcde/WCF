using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AppConfiguration.Wpf;
using WcfDomain.Contracts;
using WcfDomain.Contracts.Security;

namespace WcfClient.Commands
{
    public abstract class ClientCommand<MT, T> : RoutedCommand
        where MT : class, IContract // here we may type IContract if to update commandsManager.CommandType
        where T : class, IBroadCastContract
    {
        private readonly bool _calledFromCode;
        protected ClientInstance<MT, T> Client
        {
            get { return _client; }
        }
        private ClientInstance<MT, T> _client;
        private ClientCommandState ClientCommandState = ClientCommandState.Init;

        /// <summary>
        /// can be used in ExecuteCommand and in CanExecuteCommand
        /// </summary>
        protected Control SourceControl;

        protected ClientCommand() : this(false)
        {
        }

        /// <summary>
        /// needed for calling command from code
        /// implement this constructor in inherited class
        /// </summary>
        /// <param name="calledFromCode"></param>
        protected ClientCommand(bool calledFromCode)
        {
            _calledFromCode = calledFromCode;

            if (!calledFromCode)
            {
                Window window = Application.Current.MainWindow;
                if (window != null)
                {
                    window.CommandBindings.Add(new CommandBinding(this, Executed, CanExecute));
                }
            }
        }

        #region Channels

        public MT MainChannel
        {
            get { return _client.MainChannel; }
        }

        /// <summary>
        /// Broadcasts only to the sender
        /// </summary>
        public T MainBroadcastChannel
        {
            get { return _client.BroadcastInstance; }
        }

        public T BroadcastChannel
        {
            get { return _client.BroadcastChannels.Channel; }
        }

        #endregion

        protected void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ClientCommandState != ClientCommandState.Executing)
            {
                SourceControl = (Control) e.OriginalSource;
                if (_client == null)
                {
                    _client = TreeHelper.FindDataContext<ClientInstance<MT, T>>(SourceControl);
                }
                e.CanExecute = ((ClientCommand<MT, T>) e.Command).CanExecuteCommand(e.Parameter);
            }
        }

        internal bool CanExecuteCommand(ClientInstance<MT, T> clientEx, object par)
        {
            if (_client == null)
                _client = clientEx;
            return CanExecuteCommand(par);
        }

        protected virtual bool CanExecuteCommand(object par)
        {
            return _client != null // can be null after bad type casting
                   && _client.ClientStatus == ClientStatus.Connected;
        }

        protected void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ClientCommandState != ClientCommandState.Executing)
            {
                SourceControl = (Control) e.OriginalSource;
                ThreadPool.QueueUserWorkItem(delegate
                                                 {
                                                     var setBusy = (Action<Control, bool>) SetBusy;
                                                     SourceControl.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                     setBusy, SourceControl, true);

                                                     ExecuteSafe(_client, e.Parameter);

                                                     SourceControl.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                                     setBusy, SourceControl, false);
                                                 });
            }
        }

        private void SetBusy(Control sender, bool b)
        {
            ClientCommandState = b ? ClientCommandState.Executing : ClientCommandState.Init;
            sender.Background = b ? Brushes.Orange : Brushes.WhiteSmoke;
            if (!b)
                CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// no binding required and CanExecute not called
        /// </summary>
        internal object ExecuteSafe(ClientInstance<MT, T> clientEx, object par)
        {
            if (_client == null)
                _client = clientEx;
            else if (clientEx != null && clientEx != _client)
                throw new ApplicationException("Wrong _client parameter");
            try
            {
                Debug.Print("{2}: {1} started at {0}", DateTime.Now, GetType().Name, _client.ApplicationID);
                // if server was stopped till here there will be exception
                return ExecuteCommand(par);
            }
            catch (FaultException ex)
            {
                ClientCommandState = ClientCommandState.Fault;

                // thrown inside of server
                // channels are NOT distroyed
                if (!_calledFromCode)
                    Show(ex.Message);
                return ex.Message;
            }
            catch (PermissionException ex)
            {
                if (!_calledFromCode)
                {
                    Show(ex.Message);
                }
                return ex.Message;
            }
            catch (ApplicationException ex)
            {
                // thrown on _client inside of ExecuteCommand
                _client.LastError = ex.Message;
                if (!_calledFromCode)
                    Show(ex.Message);
                return ex.Message;
            }
                //like EndpointNotFoundException 
                //  or CommunicationObjectFaultedException(thrown if _client requests killed server)
            catch (CommunicationException ex)
            {
                ClientCommandState = ClientCommandState.Fault;
                // channels are distroyed
                _client.CreateFactories();

                Show("Server is not started", ex.GetType().Name);
                return ex.Message;
            }
            catch (ThreadAbortException)
            {
                return null;
            }
            catch (Exception ex)
            {
                ClientCommandState = ClientCommandState.Fault;

                // channels are distroyed, but server has adequate state.
                // _client may reconnect automatically or wait for user reaction
                //      _client.CreateFactories(); does not help
                Show(ex.Message, ex.GetType().Name);
                return null;
            }
            finally
            {
                Debug.Print("{2}: {0} finished at {1}", GetType().Name, DateTime.Now, _client.ApplicationID);
                // here additional session may be closed
            }
        }

        /// <summary>
        /// user interaction is allowed through Dispatcher of SourceControl
        /// </summary>
        protected abstract object ExecuteCommand(object par);

        protected static void CheckError(string s)
        {
            CheckError(s, null);
        }

        protected static void CheckError(string s, Action beforeErrorFound)
        {
            if (s != null)
            {
                if (beforeErrorFound != null) beforeErrorFound();
                throw new ApplicationException(s);
            }
        }

        protected void Show(string message)
        {
            Show(message, null);
        }

        protected void Show(string message, string title)
        {
            if (!_calledFromCode)
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher
                        .Invoke(DispatcherPriority.Normal,
                                (Action) (() => MessageBox.Show(Application.Current.MainWindow,
                                                                message)));
                else
                    MessageBox.Show(message);
            }
        }

        public static void CloseClient(ClientInstance<MT, T> c)
        {
            c.ClientStatus = ClientStatus.ChannelClosePending;
            c.CloseFactories();
        }
    }

    public enum ClientCommandState
    {
        Init,
        Executing,
        Fault,
    }
}