using System.Diagnostics;
using WcfDomain.Contracts.Clients;

namespace WcfDomain.Contracts
{
    public class UniquePropertyNotifier : PropertyNotifier
    {
        private const char separator = ':';
        public const string UniqueKeyHeader = "UniqueKey";

        private User _user;

        private UniquePropertyNotifier()
        {
        }

        public UniquePropertyNotifier(string appID)
        {
            ApplicationID = appID;
        }

        /// <summary>
        /// many applications may be hosted in the same process
        /// </summary>
        public string ApplicationID { get; private set; }

        /// <summary>
        /// CurrentUser
        /// </summary>
        public User User
        {
            get
            {
                if (_user == null)
                {
                    _user = new User();
                }
                return _user;
            }
            set // mainly for unit-tests
                // and for Delphi addon
            { _user = value; }
        }

        /// <summary>
        /// used to distinguish clients, servers
        /// </summary>
        public string UniqueKey
        {
            get
            {
                return string.Format("{0}{3}{1}{3}{2}",
                                     ApplicationID, User.ComputerName,
                                     Process.GetCurrentProcess().Id, separator);
            }
        }

        internal static string GetApplicationID(string uniqueKey)
        {
            int i = uniqueKey.LastIndexOf(separator);
            i = uniqueKey.Substring(0, i).LastIndexOf(separator);
            return uniqueKey.Substring(0, i);
        }

        public static string GetComputerName(string uniqueKey)
        {
            int b = uniqueKey.LastIndexOf(separator);
            int a = uniqueKey.Substring(0, b).LastIndexOf(separator);
            return uniqueKey.Substring(a + 1, b - a - 1);
        }
    }
}