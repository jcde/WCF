namespace WcfDomain.Contracts.Chats
{
    public enum ChatUserStatus
    {
        Available = 1,
        AvailableOutOnly = 2,
        NeedApprovalForIn = 3,
        NotAvailable = 4,
    }

    public enum ChatUser4RoomStatus
    {
        WaitingModeratorApproveToJoin = 1,
        WaitingOwnApprove = 2, // wher user is in NeedApprovalForIn status then he/she may reject invitation
    }
}