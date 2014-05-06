using System;
using System.Runtime.Serialization;

namespace WcfDomain.Contracts.Security
{
    [Serializable]
    public class PermissionException : ApplicationException
    {
        public PermissionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public PermissionException(string detail)
            : base(detail)
        {
        }

        public override string Message
        {
            get { return "Access is denied. " + base.Message; }
        }
    }
}