using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                    .Where(c => c.TenantId == tenantId).ToList();

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
                    return BadRequest("Request body is null.");
                }

                var tenantIdClaim = User.FindFirst("TenantId")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    return Unauthorized("TenantId claim is missing.");
                }

                var tenantId = Guid.Parse(tenantIdClaim);

                var newCase = new Case
                {
                    CaseNumber = request.CaseNumber,
                    ClientName = request.ClientName,
                    Status = "Open",
                    Deadline = request.DeadLine,
                    TenantId = tenantId
                };

                await _db.Cases.AddAsync(newCase);
                await _db.SaveChangesAsync();

                return Ok(newCase);
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

    }
}
