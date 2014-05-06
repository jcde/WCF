namespace WcfClient
{
    public enum ClientStatus
    {
        NotConnected,
        ConnectPending,
        Connected,
        DisconnectPending,
        ChannelClosePending,
        Refused,
    }
}