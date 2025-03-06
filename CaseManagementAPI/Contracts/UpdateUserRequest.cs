namespace CaseManagementAPI.Contracts
{
    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Role { get; set; }
    }
}
