using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes
{
    public class DocumentAttachmentRepository : IDocumentAttachmentRepository
    {
        private readonly DataWarehouseDbContext _context;
        private readonly ILogger<DocumentAttachmentRepository> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ISapCache sapCache;

        // Allowed extensions
        private readonly string[] _allowedExtensions =
        {
        ".jpg", ".jpeg", ".png", ".gif", ".pdf",
        ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip"};

        // Max file size: 10MB
        private const long MaxFileSize = 10 * 1024 * 1024;

        public DocumentAttachmentRepository(
            DataWarehouseDbContext context,
            ILogger<DocumentAttachmentRepository> logger,
            IWebHostEnvironment environment,
            ISapCache sapCache)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
            this.sapCache = sapCache;
        }

        public async Task<GeneralResponse<DocumentAttachmentDto>> UploadDocumentAsync(
        
            UploadDocumentDto dto,
            string userId)
        {
            try
            {
                var sapId = await sapCache.Get();


                // 1️⃣ Validate file
                var validation = ValidateFile(dto.File);
                if (!validation.IsValid)
                    return GeneralResponse<DocumentAttachmentDto>.FailResponse(validation.ErrorMessage);

                // 2️⃣ Generate unique filename
                var fileExtension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // 3️⃣ Create directory structure: wwwroot/uploads/{sapId}/{documentType}/{year}/{month}/
                var uploadsBasePath = Path.Combine(
                    _environment.WebRootPath ?? _environment.ContentRootPath,
                    "uploads"
                );

                var uploadPath = Path.Combine(
                    uploadsBasePath,
                    sapId.ToString(),
                    dto.DocumentType.ToString(),
                    DateTime.UtcNow.Year.ToString(),
                    DateTime.UtcNow.Month.ToString("D2")
                );

                // Ensure directory exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                    _logger.LogInformation("Created upload directory: {Path}", uploadPath);
                }

                // 4️⃣ Save file
                var filePath = Path.Combine(uploadPath, uniqueFileName);
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.File.CopyToAsync(stream);
                    }
                    _logger.LogInformation("File saved successfully: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving file to: {FilePath}", filePath);
                    throw;
                }

                // 5️⃣ Save to database
                // Store relative path for better portability
                var relativePath = Path.Combine(
                    "uploads",
                    sapId.ToString(),
                    dto.DocumentType.ToString(),
                    DateTime.UtcNow.Year.ToString(),
                    DateTime.UtcNow.Month.ToString("D2"),
                    uniqueFileName
                ).Replace('\\', '/'); // Use forward slashes for web paths

                var attachment = new DocumentAttachment
                {
                    DocumentType = dto.DocumentType,
                    DocumentId = dto.DocumentId,
                    FileName = uniqueFileName,
                    OriginalFileName = dto.File.FileName,
                    FileExtension = fileExtension,
                    FilePath = relativePath, // Store relative path
                    FileSizeBytes = dto.File.Length,
                    ContentType = dto.File.ContentType,
                    Description = dto.Description,
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = userId,
                    SapId = sapId??0,
                    IsActive = true
                };

                await _context.DocumentAttachments.AddAsync(attachment);
                await _context.SaveChangesAsync();

                // 6️⃣ Return response
                var result = MapToDto(attachment);

                _logger.LogInformation(
                    "Document uploaded: {FileName} for {DocumentType} #{DocumentId}",
                    dto.File.FileName, dto.DocumentType, dto.DocumentId
                );

                return GeneralResponse<DocumentAttachmentDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return GeneralResponse<DocumentAttachmentDto>.FailResponse("Failed to upload document");
            }
        }


        public async Task<GeneralResponse<List<NameStatus>>> GetDocumentStatus()
        {
            var statuses = Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .Select(s => new NameStatus
                {
                    Id = (int)s,
                    Name = s.ToString()
                })
                .ToList();

            return await Task.FromResult(new GeneralResponse<List<NameStatus>>
            {
                Success = true,
                Message = "Document Type retrieved successfully",
                Data = statuses
            });
        }
        public async Task<GeneralResponse<List<DocumentAttachmentDto>>> UploadMultipleDocumentsAsync(
            UploadMultipleDocumentsDto dto,
            string userId)
        {
            try
            {
                var results = new List<DocumentAttachmentDto>();
                var errors = new List<string>();

                foreach (var file in dto.Files)
                {
                    var singleDto = new UploadDocumentDto
                    {
                        DocumentType = dto.DocumentType,
                        DocumentId = dto.DocumentId,
                        File = file,
                        Description = dto.Description
                    };

                    var result = await UploadDocumentAsync(singleDto, userId);

                    if (result.Success)
                        results.Add(result.Data);
                    else
                        errors.Add($"{file.FileName}: {result.Message}");
                }

                if (errors.Any())
                {
                    return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse(
                        $"Some files failed: {string.Join(", ", errors)}",
                        errors
                    );
                }

                return GeneralResponse<List<DocumentAttachmentDto>>.SuccessResponse(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple documents");
                return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse("Failed to upload documents");
            }
        }

        public async Task<GeneralResponse<List<DocumentAttachmentDto>>> GetDocumentsByTypeAndIdAsync(
           
            DocumentType documentType,
            int documentId)
        {
            try
            {
                var sapId = await sapCache.Get();

                var attachments = await _context.DocumentAttachments
                    .Where(x => x.SapId == sapId
                             && x.DocumentType == documentType
                             && x.DocumentId == documentId
                             && x.IsActive)
                    .OrderByDescending(x => x.UploadedAt)
                    .ToListAsync();

                var results = attachments.Select(MapToDto).ToList();

                return GeneralResponse<List<DocumentAttachmentDto>>.SuccessResponse(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse("Failed to get documents");
            }
        }

        public async Task<GeneralResponse<byte[]>> DownloadDocumentAsync(
          
            int documentAttachmentId)
        {
            try
            {
                var sapId = await sapCache.Get();

                var attachment = await _context.DocumentAttachments
                    .FirstOrDefaultAsync(x => x.DocumentAttachmentId == documentAttachmentId
                                           && x.SapId == sapId
                                           && x.IsActive);

                if (attachment == null)
                    return GeneralResponse<byte[]>.FailResponse("Document not found");

                // Resolve full path from relative path
                var fullPath = Path.IsPathRooted(attachment.FilePath) 
                    ? attachment.FilePath 
                    : Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachment.FilePath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found at path: {FilePath}", fullPath);
                    return GeneralResponse<byte[]>.FailResponse("File not found on server");
                }

                var fileBytes = await File.ReadAllBytesAsync(fullPath);

                return GeneralResponse<byte[]>.SuccessResponse(fileBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document");
                return GeneralResponse<byte[]>.FailResponse("Failed to download document");
            }
        }

        public async Task<GeneralResponse<bool>> DeleteDocumentAsync(
           
            int documentAttachmentId)
        {
            try
            {
                var sapId = await sapCache.Get();
                var attachment = await _context.DocumentAttachments
                    .FirstOrDefaultAsync(x => x.DocumentAttachmentId == documentAttachmentId
                                           && x.SapId == sapId);

                if (attachment == null)
                    return GeneralResponse<bool>.FailResponse("Document not found");

                // Soft delete
                attachment.IsActive = false;
                await _context.SaveChangesAsync();

                // Optional: Delete physical file
                try
                {
                    var fullPath = Path.IsPathRooted(attachment.FilePath)
                        ? attachment.FilePath
                        : Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachment.FilePath);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation("Physical file deleted: {FilePath}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete physical file: {FilePath}", attachment.FilePath);
                    // Continue even if physical file deletion fails
                }

                _logger.LogInformation("Document deleted: {DocumentId}", documentAttachmentId);

                return GeneralResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return GeneralResponse<bool>.FailResponse("Failed to delete document");
            }
        }

        // Helper Methods
        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "File is empty");

            if (file.Length > MaxFileSize)
                return (false, $"File size exceeds {MaxFileSize / 1024 / 1024}MB limit");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return (false, $"File type {extension} is not allowed");

            return (true, string.Empty);
        }

        private DocumentAttachmentDto MapToDto(DocumentAttachment attachment)
        {
            return new DocumentAttachmentDto
            {
                DocumentAttachmentId = attachment.DocumentAttachmentId,
                DocumentType = attachment.DocumentType,
                DocumentName = ((DocumentType)attachment.DocumentType).ToString(),
                DocumentId = attachment.DocumentId,
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                FileExtension = attachment.FileExtension,
                FileSizeBytes = attachment.FileSizeBytes,
                FileSizeFormatted = FormatFileSize(attachment.FileSizeBytes),
                ContentType = attachment.ContentType,
                Description = attachment.Description,
                UploadedAt = attachment.UploadedAt,
                UploadedBy = attachment.UploadedBy,
                DownloadUrl = $"/api/documents/{attachment.DocumentAttachmentId}/download"
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public async Task<DocumentAttachmentDto?> GetDocumentAttachmentByIdAsync(
          
            int documentAttachmentId)
        {
            var sapId = await sapCache.Get();

            var attachment = await _context.DocumentAttachments
                .FirstOrDefaultAsync(x => x.DocumentAttachmentId == documentAttachmentId
                                       && x.SapId == sapId
                                       && x.IsActive);

            if (attachment == null)
                return null;

            return MapToDto(attachment);
        }
    }
}
