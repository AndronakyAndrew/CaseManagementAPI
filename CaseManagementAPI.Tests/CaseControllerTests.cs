using CaseManagementAPI.Controllers;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace CaseManagementAPI.Tests
{
    public class CaseControllerTests
    {
        [Fact]
        public void GetCases_ReturnsOkResult_WithCases()
        {
            // Arrange
            var mockDbContext = new Mock<AppDBContext>();
            var tenantId = Guid.NewGuid();
            var cases = new List<Case>
            {
                new Case { CaseId = Guid.NewGuid(), CaseNumber = "123", ClientName = "Client A", Status = "Open", Deadline = DateTime.Now, TenantId = tenantId },
                new Case { CaseId = Guid.NewGuid(), CaseNumber = "456", ClientName = "Client B", Status = "Open", Deadline = DateTime.Now, TenantId = tenantId }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Case>>();
            mockDbSet.As<IQueryable<Case>>().Setup(m => m.Provider).Returns(cases.Provider);
            mockDbSet.As<IQueryable<Case>>().Setup(m => m.Expression).Returns(cases.Expression);
            mockDbSet.As<IQueryable<Case>>().Setup(m => m.ElementType).Returns(cases.ElementType);
            mockDbSet.As<IQueryable<Case>>().Setup(m => m.GetEnumerator()).Returns(cases.GetEnumerator());

            mockDbContext.Setup(db => db.Cases).Returns(mockDbSet.Object);

            var controller = new CasesController(mockDbContext.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("TenantId", tenantId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = controller.GetCases();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnCases = Assert.IsType<List<Case>>(okResult.Value);
            Assert.Equal(2, returnCases.Count);
        }

        [Fact]
        public void GetCases_ReturnsUnauthorized_WhenTenantIdClaimIsMissing()
        {
            // Arrange
            var mockDbContext = new Mock<AppDBContext>();
            var controller = new CasesController(mockDbContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var result = controller.GetCases();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("TenantId claim is missing.", unauthorizedResult.Value);
        }

        [Fact]
        public void GetCases_ReturnsBadRequest_WhenTenantIdIsInvalid()
        {
            // Arrange
            var mockDbContext = new Mock<AppDBContext>();
            var controller = new CasesController(mockDbContext.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("TenantId", "invalid-guid")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = controller.GetCases();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid TenantId format.", badRequestResult.Value);
        }
    }
}
