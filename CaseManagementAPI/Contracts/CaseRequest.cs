namespace CaseManagementAPI.Contracts
{
    public class CaseRequest
    {
        public string CaseNumber { get; set; }
        public string ClientName { get; set; }
        public DateTime DeadLine { get; set; }
    }
}
