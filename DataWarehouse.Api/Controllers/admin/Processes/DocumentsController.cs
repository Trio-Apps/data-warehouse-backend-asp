using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace DataWarehouse.Api.Controllers.admin.Processes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentAttachmentRepository _service;

        public DocumentsController(IDocumentAttachmentRepository service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto dto)
        {
            // Manual validation for file upload
            if (dto == null)
                return BadRequest(new { error = "Request body is required" });

            // Skip ModelState validation for File field - we handle it manually
            ModelState.Remove(nameof(dto.File));

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { error = "File is required and cannot be empty" });

            if (dto.DocumentId <= 0)
                return BadRequest(new { error = "DocumentId is required and must be greater than 0" });

            if (!Enum.IsDefined(typeof(DocumentType), dto.DocumentType))
                return BadRequest(new { error = "Invalid DocumentType" });

           
          

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "UserId is required" });

            var result = await _service.UploadDocumentAsync(dto, userId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleDocuments([FromForm] UploadMultipleDocumentsDto dto)
        {
            // Manual validation for multiple file upload
            if (dto == null)
                return BadRequest(new { error = "Request body is required" });

            // Skip ModelState validation for Files field - we handle it manually
            ModelState.Remove(nameof(dto.Files));

            if (dto.Files == null || !dto.Files.Any())
                return BadRequest(new { error = "At least one file is required" });

            if (dto.DocumentId <= 0)
                return BadRequest(new { error = "DocumentId is required and must be greater than 0" });

            if (!Enum.IsDefined(typeof(DocumentType), dto.DocumentType))
                return BadRequest(new { error = "Invalid DocumentType" });

           
            
         
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "UserId is required" });

            var result = await _service.UploadMultipleDocumentsAsync( dto, userId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{documentType}/{documentId}")]
        public async Task<IActionResult> GetDocuments(DocumentType documentType, int documentId)
        {
           

            var result = await _service.GetDocumentsByTypeAndIdAsync( documentType, documentId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{documentAttachmentId}/download")]
        public async Task<IActionResult> DownloadDocument(int documentAttachmentId)
        {
         

            var result = await _service.DownloadDocumentAsync( documentAttachmentId);

            if (!result.Success)
                return BadRequest(result);

            var attachment = await _service.GetDocumentAttachmentByIdAsync( documentAttachmentId);

            if (attachment == null)
                return NotFound("Attachment not found");

            return File(result.Data, attachment.ContentType, attachment.OriginalFileName);
        }

        [HttpDelete("{documentAttachmentId}")]
        public async Task<IActionResult> DeleteDocument(int documentAttachmentId)
        {
           

            var result = await _service.DeleteDocumentAsync( documentAttachmentId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        //

        [HttpGet("status")]
        public async Task<IActionResult> DocumentStatus()
        {
            var result = await _service.GetDocumentStatus();
         
            if (!result.Success)
                return BadRequest(result);
            if (result == null)
                return NotFound("Status not found");

            return Ok(result);
        }

        // Helper methods
        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}
