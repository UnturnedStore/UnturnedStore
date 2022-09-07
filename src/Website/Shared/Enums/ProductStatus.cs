using System.ComponentModel;

namespace Website.Shared.Enums
{
    public enum ProductStatus
    {
        New = 0,
        [Description("Waiting For Approval")]
        WaitingForApproval = 1,
        Approved = 2,
        Rejected = 3,
        Released = 4,
        Disabled = 5
    }
}
