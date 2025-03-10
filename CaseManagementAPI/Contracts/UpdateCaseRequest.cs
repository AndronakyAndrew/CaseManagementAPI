namespace CaseManagementAPI.Contracts
{
    public class UpdateCaseRequest
    {
        public string Status { get; set; }
        public DateTime DeadLine { get; set; }
    }
}
