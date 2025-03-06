using CaseManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementAPI.Data
{
    public class AppDBContext : DbContext
    {
        public required DbSet<Tenant> Tenants { get; set; }
        public required DbSet<User> Users { get; set; }
        public required virtual DbSet<Case> Cases { get; set; }
        public required DbSet<Document> Documents { get; set; }
        public required DbSet<Subscription> Subscriptions { get; set; }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public AppDBContext() { }
    }
}
