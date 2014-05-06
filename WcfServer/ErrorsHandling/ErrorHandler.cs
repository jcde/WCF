using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using WcfDomain.Contracts;
using WcfServer.Services;

namespace WcfServer.ErrorsHandling
{
    /// <summary>
    /// server-side handler
    /// </summary>
    public class ErrorHandler<T> : IErrorHandler
        where T : class, IBroadCastContract
    {
        internal DuplexService<T> Service;

        public ErrorHandler()
        {
            //ServiceModel.AsynchronousThreadExceptionHandler +=
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        #region IErrorHandler Members

        /// <summary>
        /// called in separate thread intended for logging only
        /// </summary>
        public bool HandleError(Exception error)
        {
            //logging here will give more details than in ProvideFault(...)        
            //  exceptions (like, channel fault) may be catched in HandleError(...)
            //      if ClientControl was not closed with Dispose()
            //uncomment to get more details Logger.Write(error);
            if (error is FaultException)
            {
                return false; // Let WCF do normal processing
            }
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var f = error as FaultException;
            if (f == null)
            {
                MessageFault messageFault = MessageFault.CreateFault(
                    new FaultCode("Sender"), new FaultReason(error.Message), error, new NetDataContractSerializer());
                fault = Message.CreateMessage(version, messageFault, null);
            }
            LogError(error);
            //BroadcastError(f ?? new FaultException(error.Message));
        }

        #endregion

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError(e.ExceptionObject as Exception);
            if (e.IsTerminating)
                if (Service != null)
                {
                    // server is terminating 
                    Service.BroadcastsChannel.ServerError(
                        new FaultException(e.ExceptionObject.ToString()));
                }
        }

        private void LogError(Exception e)
        {
            Logger.Write(e);
            BroadcastError(e as FaultException ?? new FaultException(e.ToString()));
        }

        /// <param name="e">NOT of type Exception</param>
        private void BroadcastError(FaultException e)
        {
            if (Service != null)
            {
                try
                {
                    if (Service.SenderBroadcastsChannel != null
                        &&
                        ((ICommunicationObject) Service.SenderBroadcastsChannel).State ==
                        CommunicationState.Opened)
                    {
                        // initiator should be notified about error
                        Service.SenderBroadcastsChannel.ServerError(new FaultException(e.Message));
                    }
                    else
                    {
                        // if there is no Initiator then everyone will be informed about error
                        Service.BroadcastsChannel.ServerError(new FaultException(e.Message));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
        }
    }
}