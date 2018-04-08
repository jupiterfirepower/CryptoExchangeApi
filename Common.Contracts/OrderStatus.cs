namespace Common.Contracts
{
    public enum OrderStatus
    {
        PendingNew = 0,
        New = 1,
        PartiallyFilled = 2,
        Filled = 3,
        DoneForDay = 4,
        Canceled = 5,
        PendingCancel = 6,
        Stopped = 7,
        Rejected = 8,
        Suspended = 9,
        Calculated = 10,
        Expired = 11,
        AcceptedForBidding = 12,
        PendingReplace = 13,
        Replaced = 14,
        Unknown = 100// TO BE REMOVED
    }
}
