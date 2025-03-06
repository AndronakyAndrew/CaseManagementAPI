namespace CaseManagementAPI.Models
{
    public class Case
    {
        public Guid CaseId { get; set; } = Guid.NewGuid();
        public string CaseNumber { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; } // Open, Completed 
        public DateTime Deadline { get; set; } = DateTime.UtcNow;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
