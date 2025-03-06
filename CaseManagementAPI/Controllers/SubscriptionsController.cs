using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementAPI.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("subscriptions")]

    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDBContext _db;

        public SubscriptionsController(AppDBContext db)
        {
            _db = db;
        }

        //Get subscription for a tenant
        [HttpGet]
        public async Task<IActionResult> GetSubscription()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription == null)
                return NotFound("Подписки не куплена или не продлена.");

            return Ok(new
            {
                subscription.Plan,
                subscription.StartDate,
                subscription.EndDate,
                subscription.IsActive
            });
        }

        //Update subscription plan
        [HttpPost("update")]
        public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription == null)
                return NotFound("Подписка не найдена.");

            subscription.Plan = request.Plan;
            subscription.EndDate = DateTime.Now.AddMonths(1);
            subscription.IsActive = true;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                subscription.Plan,
                subscription.StartDate,
                subscription.EndDate,
                subscription.IsActive
            });
        }
    }
}
