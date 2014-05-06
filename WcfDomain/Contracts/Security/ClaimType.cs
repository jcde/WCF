namespace WcfDomain.Contracts.Security
{
    public enum ClaimType
    {
        Read,
        Write,
        Execute,
        All = Read | Write | Execute,
    }
}