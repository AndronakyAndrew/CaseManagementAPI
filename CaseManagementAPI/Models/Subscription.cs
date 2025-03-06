namespace CaseManagementAPI.Models
{
    public class Subscription
    {
        public Guid SubscriptionId { get; set; } = Guid.NewGuid();
        public string Plan { get; set; } //'Basic', 'Premium', 'Enterprise'
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
