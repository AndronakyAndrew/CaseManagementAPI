using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CaseManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("documents")]
    public class DocumentController : ControllerBase
    {
        private readonly AppDBContext _db;
        private readonly IWebHostEnvironment _env;

        public DocumentController(AppDBContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        //Upload a document for a specific case
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] Guid caseId, [FromForm] IFormFile file)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var caseEntity = await _db.Cases.FirstOrDefaultAsync(c => c.CaseId == caseId && c.TenantId == tenantId);

            if (caseEntity == null)
                return NotFound("Case not found.");

            var uploadDir = Path.Combine(_env.ContentRootPath, "files", tenantId.ToString(), caseId.ToString());
            Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, $"{Guid.NewGuid()}_{file.FileName}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                CaseId = caseId,
                FileName = file.FileName,
                FilePath = filePath,
                UploadedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                TenantId = tenantId
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            return Ok(document);
        }

        //Download a specific document
        [HttpGet("{documentId}")]
        public async Task<IActionResult> DownloadDocument(Guid documentId)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var document = await _db.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.TenantId == tenantId);

            if (document == null)
                return NotFound("No such document");

            var fileBytes = System.IO.File.ReadAllBytes(document.FilePath);
            return File(fileBytes, "application/octet-stream", document.FileName);
        }

        //List all documents for a specific case
        [HttpGet("case/{caseId}")]
        public async Task<IActionResult> ListDocuments(Guid caseId)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var documents = await _db.Documents
                .Where(d => d.CaseId == caseId && d.TenantId == tenantId)
                .Select(d => new { d.DocumentId, d.FileName, d.UploadedBy })
                .ToListAsync();

            return Ok(documents);
        }

        //Delete a specific document
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocuments(Guid documentId)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var document = await _db.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.TenantId == tenantId);

            if(document == null) 
                return NotFound("Document does not exist!");

            System.IO.File.Delete(document.FilePath);
            _db.Documents.Remove(document);
            await _db.SaveChangesAsync();

            return Ok("Document deleted successfully.");
        }
    }
}
