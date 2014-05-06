using System;
using System.ServiceModel;
using System.Threading;
using ServiceModelEx;
using WcfDomain.Properties;

namespace WcfDomain.Contracts
{
    public class Channels
    {
        private static bool IsOpenAllowed(CommunicationState state)
        {
            return state == CommunicationState.Created
                   || state == CommunicationState.Closed
                   || state == CommunicationState.Faulted;
        }

        public static T Open<T>(ref ICommunicationObject channel, ChannelFactory<T> factory)
        {
            return Open(ref channel, factory, null);
        }

        public static T Open<T>(ref ICommunicationObject channel, ChannelFactory<T> factory,
                                Action beforeOpen)
        {
            if ((channel == null || IsOpenAllowed(channel.State))
                && factory != null)
            {
                if (IsOpenAllowed(factory.State))
                {
                    if (beforeOpen != null)
                    {
                        beforeOpen();
                    }
                    factory.Open();
                }

                if (channel == null || channel.State == CommunicationState.Faulted)
                    channel = (ICommunicationObject) factory.CreateChannel();
                if (channel != null) // may be null during AutoConnection on stopped server
                    lock (channel)
                        if (IsOpenAllowed(channel.State))
                        {
                            // in this moment ConcurrencyMode.Reentrant FREES service instance
                            channel.Open();
                            // we have to open explicitly because auto-open is CONSEQUTIVE and is not parallel
                        }
            }
            else
                // other too smart thread must wait until Open() will end
                while (channel != null && channel.State == CommunicationState.Opening)
                {
                    Thread.Sleep(100);
                }
            return (T) channel;
        }

        public static void CloseChannel(ref ICommunicationObject channel)
        {
            Close(channel);
            channel = null;
        }

        public static void CloseFactory<T>(ref ChannelFactory<T> factory)
        {
            Close(factory);
            factory = null;
        }

        public static void CloseFactory<T, B>(ref DuplexChannelFactory<T, B> factory) where T : class
        {
            Close(factory);
            factory = null;
        }

        private static void Close(ICommunicationObject channel)
        {
            if (channel != null)
            {
                if (channel.State == CommunicationState.Opened)
                    try
                    {
                        channel.Close();
                    }
                    catch (TimeoutException)
                    {
                        channel.Abort();
                    }
                    catch
                    {
                        // exception wil be thrown if endpoint is opened and in faulted state
                    }
            }
        }

        public static NetTcpBinding GetBinding()
        {
            return GetBinding(10);
        }

        public static NetTcpBinding GetBinding(int maxConnections)
        {
            var binding = new NetTcpBinding
                              {
                                  ReceiveTimeout = TimeSpan.MaxValue,
                                  MaxConnections = maxConnections,
                              };

            binding.MaxReceivedMessageSize = binding.MaxBufferPoolSize
                                             = binding.ReaderQuotas.MaxArrayLength = binding.MaxBufferSize
                                                                                     = 10000000; // 10MB

            binding.Security.Mode = (SecurityMode) Enum.Parse(typeof (SecurityMode),
                                                              Settings.Default.WcfSecurityMode);
//            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            return binding;
        }
    }
}