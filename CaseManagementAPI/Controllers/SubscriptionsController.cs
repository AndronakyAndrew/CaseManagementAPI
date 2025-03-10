using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                return NotFound("Подписка не куплена или не продлена.");

            return Ok(new
            {
                subscription.Plan,
                subscription.StartDate,
                subscription.EndDate,
                subscription.IsActive
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSubscription([FromForm] AddSubscription request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription != null)
                return Conflict("Подписка уже куплена или продлена.");

            //Payment section will be develop soon
            

            var newSubscription = new Subscription
            {
                TenantId = tenantId,
                Plan = request.Plan,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true
            };
            await _db.Subscriptions.AddAsync(newSubscription);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                newSubscription.Plan,
                newSubscription.StartDate,
                newSubscription.EndDate,
                newSubscription.IsActive
            });
        }
        //Update subscription plan
        [HttpPut("update")]
        public async Task<IActionResult> UpdateSubscription([FromForm] UpdateSubscriptionRequest request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId);

            if (subscription == null)
                return NotFound("Подписка не найдена.");

            subscription.Plan = request.Plan;
            subscription.EndDate = DateTime.UtcNow.AddMonths(1);
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

        //Cancel subscription
        [HttpDelete("cancel")]
        public async Task<IActionResult> CancelSubscription(Guid subscriptionId)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim))
                return Unauthorized("TenantId claim is missing.");

            var tenantId = Guid.Parse(tenantIdClaim);

            var subscription = await _db.Subscriptions
                .Where(s => s.SubscriptionId == subscriptionId && s.TenantId == tenantId)
                .ExecuteDeleteAsync();

            return Ok(subscriptionId);
        }
    }
}
