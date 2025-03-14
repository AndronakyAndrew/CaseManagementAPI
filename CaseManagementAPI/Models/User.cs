﻿namespace CaseManagementAPI.Models
{
    public class User
    {
        public Guid UserId {  get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
