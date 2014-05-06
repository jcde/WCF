using System;
using System.Runtime.Serialization;
using System.Security;

namespace WcfDomain.Contracts.Clients
{
    [DataContract(Namespace = Namespaces.Data)]
    public class User
    {
        public User()
        {
            Name = Environment.UserName;
            Domain = Environment.UserDomainName;
            ComputerName = Environment.MachineName;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Domain { get; set; }

        /// <summary>
        /// computer where client is located
        /// </summary>
        [DataMember]
        public string ComputerName { get; set; }

        /// <summary>
        /// includes domain separated by \ symbol
        /// </summary>
        public string NameWithDomain
        {
            get
            {
                return string.Format("{0}{1}{2}",
                                     Domain, (string.IsNullOrEmpty(Domain) ? "" : @"\"), Name);
            }
            set
            {
                if (value != null && value.Contains(@"\"))
                {
                    Domain = value.Substring(0, value.IndexOf('\\'));
                    Name = value.Substring(value.IndexOf('\\') + 1);
                }
                else
                {
                    Domain = null;
                    Name = value;
                }
            }
        }

        public string Password { get; set; }

        public SecureString PasswordSecure
        {
            get
            {
                var ss = new SecureString();
                foreach (char c in Password)
                    ss.AppendChar(c);
                return ss;
            }
        }

        public override string ToString()
        {
            return NameWithDomain;
        }

        #region UniqueKey

        public override bool Equals(object obj)
        {
            if (obj is User)
            {
                var ob = (User) obj;
                return NameWithDomain.Equals(ob.NameWithDomain);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return NameWithDomain.GetHashCode();
        }

        public static bool operator ==(User a, User b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(User a, User b)
        {
            return !(a == b);
        }

        #endregion
    }
}