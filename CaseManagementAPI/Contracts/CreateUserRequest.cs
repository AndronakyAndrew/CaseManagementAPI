﻿namespace CaseManagementAPI.Contracts
{
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // 'Admin' or 'User'
    }
}
