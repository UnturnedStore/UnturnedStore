using System.ComponentModel;

namespace Website.Shared.Enums
{
    public enum ProductStatus
    {
        New,
        [Description("Waiting For Approval")]
        WaitingForApproval,
        Approved,
        Rejected,
        Released,
        Disabled
    }
}
