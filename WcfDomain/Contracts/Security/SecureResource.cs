namespace WcfDomain.Contracts.Security
{
    public enum SecureResource
    {
        ServerStartStop,
        MessageToAll,
        UserManagement,
        All = ServerStartStop | MessageToAll | UserManagement,
    }
}