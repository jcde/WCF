namespace WcfServer.Services
{
    public enum ServiceState
    {
        Starting, // no interaction with clients
        Run, // full-functional
        Closing, // new clients' connections are forbidden
        Closed,
    }
}