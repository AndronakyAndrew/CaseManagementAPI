namespace CaseManagementAPI.Models
{
    public class Tenant
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
