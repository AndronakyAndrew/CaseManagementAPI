using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("cases")]
    public class CasesController : ControllerBase
    {
        private readonly AppDBContext _db;

        public CasesController(AppDBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetCases()
        {
            try
            {
                var tenantIdClaim = User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    return Unauthorized("TenantId claim is missing.");
                }

                var tenantId = Guid.Parse(tenantIdClaim);
                var cases = _db.Cases
                    .Where(c => c.TenantId == tenantId)
                    .ToList();

                return Ok(cases);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid TenantId format.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        [HttpPost("add")]
        public async Task<IActionResult> CreateCase([FromBody] CaseRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Введены некорректные данные!");
                }

                var tenantIdClaim = User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    return Unauthorized("TenantId claim is missing.");
                }

                var tenantId = Guid.Parse(tenantIdClaim);

                if (request.DeadLine == DateTime.MinValue)
                    return BadRequest("Invalid deadline provided.");

                var tenant = await _db.Tenants.FindAsync(tenantId);

                if (tenant == null)
                    return NotFound("Tenant not found.");

                var newCase = new Case
                {
                    CaseNumber = request.CaseNumber,
                    ClientName = request.ClientName,
                    Status = "Open",
                    Deadline = request.DeadLine.ToUniversalTime(),
                    TenantId = tenantId,
                    Tenant = tenant
                };

                await _db.Cases.AddAsync(newCase);
                await _db.SaveChangesAsync();

                return Ok(newCase);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid TenantId format.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPut("update/{caseId}")]
        public async Task<IActionResult> UpdateCase(Guid caseId, [FromBody] UpdateCaseRequest request)
        {
            try
            {
                var tenantIdClaim = User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim))
                    return Unauthorized("TenantId claim is missing.");

                var tenantId = Guid.Parse(tenantIdClaim);

                
                var caseEntity = await _db.Cases
                   .Where(c => c.CaseId == caseId && c.TenantId == tenantId)
                  .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.Status, request.Status)
                        .SetProperty(c => c.Deadline, request.DeadLine.ToUniversalTime()));

                return Ok(caseEntity);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("delete/{caseId}")]
        public async Task<IActionResult> DeleteCase(Guid caseId)
        {
            try
            {
                var tenantIdClaim = User.FindFirst("TenantId")?.Value;

                if (string.IsNullOrEmpty(tenantIdClaim))
                    return Unauthorized("TenantId claim is missing.");

                var tenantId = Guid.Parse(tenantIdClaim);

                var caseEntity1 = await _db.Cases
                    .Where(c => c.CaseId.Equals(caseId) && c.TenantId.Equals(tenantId))
                    .ExecuteDeleteAsync();

                return Ok(caseId);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
