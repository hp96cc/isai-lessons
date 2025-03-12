using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ISubscriptionCode : IBaseInterface
    {
        string Code { get; set; }
        string IssuedTo { get; set; }
        int LicenceDays { get; set; }
        DateTimeOffset LicenceExpiryDate { get; set; }
        int? SubscriptionId { get; set; }
        int SubscriptionTypeId { get; set; }
        DateTimeOffset? UsedDateTime { get; set; }
        DateTimeOffset ValidFrom { get; set; }
        DateTimeOffset? ValidTo { get; set; }
    }
}