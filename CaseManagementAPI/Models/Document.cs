namespace CaseManagementAPI.Models
{
    public class Document
    {
        public Guid DocumentId { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public Guid CaseId { get; set; }
        public Case Case { get; set; }

        public Guid UploadedBy { get; set; }
        public User Uploader { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
