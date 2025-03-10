using CaseManagementAPI.Controllers;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using System.Security.Claims;

namespace CaseManagementAPI.Tests
{
    public class SubscriptionControllerTests
    {
        [Fact]
        public async Task GetSubscription_ReturnsSubscription()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var data = new List<Subscription>
            {
                new Subscription { TenantId = tenantId, Plan = "Basic", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), IsActive = true }
            };

            var mockSet = new Mock<DbSet<Subscription>>();
            mockSet.As<IQueryable<Subscription>>().Setup(m => m.Provider);
            mockSet.As<IQueryable<Subscription>>().Setup(m => m.Expression);
            mockSet.As<IQueryable<Subscription>>().Setup(m => m.ElementType);
            mockSet.As<IQueryable<Subscription>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            var mockContext = new Mock<AppDBContext>();
            mockContext.Setup(m => m.Subscriptions).Returns(mockSet.Object);

            mockSet.Setup(m => m.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription model, CancellationToken token) =>
                {
                    return Mock.Of<EntityEntry<Subscription>>();
                });

            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var controller = new SubscriptionsController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                                new Claim("TenantId", tenantId.ToString())
                    }))
                }
            };

            // Act
            var result = await controller.GetSubscription();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<Subscription>(okResult.Value);
            var subscription = data.First();
            Assert.Equal(subscription.Plan, model.Plan);
            Assert.Equal(subscription.StartDate, model.StartDate);
            Assert.Equal(subscription.EndDate, model.EndDate);
            Assert.Equal(subscription.IsActive, model.IsActive);

        }
    }
}

    
