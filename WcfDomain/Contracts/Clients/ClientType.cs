using System;

namespace WcfDomain.Contracts.Clients
{
    [Flags]
    public enum ClientType
    {
        Any = -1,
        SendAndReceiver = 0,
        Sender = 1,
        Receiver = 2,
        SendAndReceiver_WithoutChat = 16 | SendAndReceiver,
    }
}