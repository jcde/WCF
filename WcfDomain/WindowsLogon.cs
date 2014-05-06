using System;
using System.Runtime.InteropServices;

namespace WcfDomain
{
    public class WindowsLogon
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(
            string principal,
            string authority,
            string password,
            LogonTypes logonType,
            LogonProviders logonProvider,
            out IntPtr token);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        public static bool LogonUser(string nameWithDomain, string password)
        {
            string[] ss = nameWithDomain.Split('\\');
            if (ss[0].Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return LogonLocalUser(ss[1], password);
            }
            return LogonUser(ss[1], ss[0], password);
        }

        public static bool LogonLocalUser(string name, string password)
        {
            return LogonUser(name, ".", password);
        }

        public static bool LogonUser(string name,
                                     string domain, string password)
        {
            IntPtr token;
            return LogonUser(name, domain, password,
                             LogonTypes.Interactive, LogonProviders.Default, out token);
        }

        #region Nested type: LogonProviders

        private enum LogonProviders : uint
        {
            Default = 0, // default for platform (use this!)
            WinNT35, // sends smoke signals to authority
            WinNT40, // uses NTLM
            WinNT50 // negotiates Kerb or NTLM
        }

        #endregion

        #region Nested type: LogonTypes

        private enum LogonTypes : uint
        {
            Interactive = 2,
            Network,
            Batch,
            Service,
            NetworkCleartext = 8,
            NewCredentials
        }

        #endregion
    }
}