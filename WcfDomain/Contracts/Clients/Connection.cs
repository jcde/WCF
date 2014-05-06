using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WcfDomain.Contracts.Clients
{
    /// <summary>
    /// every User can launch many client appication. 
    /// For every client application the Connection's instance is associated.
    /// this data is public for all connected clients 
    /// </summary>
    [DataContract(Namespace = Namespaces.Data)]
    public class Connection : PropertyNotifier
    {
        public const string TokenHeader = "TokenHeader";

        [DataMember(Order = 1)]
        public string ClientUniqueKey { get; set; }

        [DataMember(Order = 2)]
        public User User { get; set; }

        [DataMember(Order = 3)]
        public DateTime? ConnectTime { get; set; }

        [DataMember(Order = 4)]
        public DateTime? LastReceiveDate { get; set; }

        [DataMember(Order = 5)]
        public ClientType ClientType { get; set; }

        [DataMember(Order = 6)]
        public string IP { get; set; }

        public string AppID
        {
            get { return UniquePropertyNotifier.GetApplicationID(ClientUniqueKey); }
        }

        /// <summary>
        /// empty connection with most data that can be collected always
        /// </summary>
        public static Connection Create()
        {
            string ip = null;
            foreach (IPAddress s in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (s.AddressFamily == AddressFamily.InterNetwork)
                    ip = s.ToString();
            }

            return new Connection
                       {
                           User = new User(),
                           IP = ip,
                       };
        }

        public override string ToString()
        {
            return ClientUniqueKey;
        }

        #region For display only

        private string _lastMessage;

        public string LastMessage
        {
            get { return _lastMessage; }
            set
            {
                _lastMessage = value;
                Refresh("LastMessage");
            }
        }

        #endregion

        #region For Server-side only

        /// <summary>
        /// typeof IBroadCastContract (that is strange...)
        /// </summary>
        public IContextChannel ServerChannel;

        /// <summary>
        /// may be faulted with SystemException
        /// therefore call it inside try-catch
        /// </summary>
        /// <remarks>destigned to be not property in order not to be displayed</remarks>
        public T BroadcastChannel<T>() where T : IBroadCastContract
        {
            return (T) ServerChannel;
        }

        #endregion

        #region UniqueKey

        public override bool Equals(object obj)
        {
            if (obj is Connection)
            {
                var ob = (Connection) obj;
                return ClientUniqueKey.Equals(ob.ClientUniqueKey);
            }
            if (obj is string)
                return ClientUniqueKey.Equals((string) obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (ClientUniqueKey ?? "").GetHashCode();
        }

        #endregion
    }
}