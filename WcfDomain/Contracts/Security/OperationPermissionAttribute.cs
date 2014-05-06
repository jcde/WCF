using System;
using System.Security.Permissions;

namespace WcfDomain.Contracts.Security
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OperationPermissionAttribute : Attribute
    {
        public ClaimType ClaimType = ClaimType.All;
        public SecureResource SecureResource = SecureResource.All;
        public SecurityAction SecurityAction = SecurityAction.Demand;
    }
}