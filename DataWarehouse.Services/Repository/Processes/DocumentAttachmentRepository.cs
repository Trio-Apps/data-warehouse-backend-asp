using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Core.Interfaces.ISap;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
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
using System.Xml.Linq;

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
        ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip"
        };



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


        public async Task<GeneralResponse<List<DocumentAttachmentDto>>> UploadFilesForDocumentAsync(
        string userId,
        UploadDocumentDto dto)
        {
            var savedPhysicalPaths = new List<string>();

            try
            {
                if (dto.Files == null || !dto.Files.Any())
                    return GeneralResponse<List<DocumentAttachmentDto>>.SuccessResponse(new List<DocumentAttachmentDto>());


                var sapId = await sapCache.Get();

                if (sapId == null || sapId <= 0)
                    return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse("SAP is not selected");

                var attachments = new List<DocumentAttachment>();

                if (!Enum.TryParse<ProcessType>(dto.DocumentType, true, out var processType))
                {
                    throw new Exception($"Invalid document type: {dto.DocumentType}");
                }

                var now = DateTime.UtcNow;

                var uploadsBasePath = Path.Combine(
                    _environment.WebRootPath ?? _environment.ContentRootPath
                   
                );

                var uploadPath = Path.Combine(
                    uploadsBasePath,
                     "uploads",
                    sapId.Value.ToString(),
                    dto.DocumentType.ToString(),
                    now.Year.ToString(),
                    now.Month.ToString("D2")
                );



                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                    _logger.LogInformation("Created upload directory: {Path}", uploadPath);
                }

                foreach (var file in dto.Files)
                {
                    var validation = ValidateFile(file);
                    if (!validation.IsValid)
                    {
                        return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse(
                            $"File '{file.FileName}' is invalid: {validation.ErrorMessage}");
                    }

                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var physicalPath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedPhysicalPaths.Add(physicalPath);

                    var relativePath = Path.Combine(
                        "uploads",
                        sapId.Value.ToString(),
                        dto.DocumentType.ToString(),
                        now.Year.ToString(),
                        now.Month.ToString("D2"),
                        uniqueFileName
                    ).Replace('\\', '/');

                    var attachment = new DocumentAttachment
                    {
                        DocumentType = processType,
                        DocumentId = dto.DocumentId,
                        SourcePath = uploadsBasePath,
                        FileName = uniqueFileName,
                        OriginalFileName = file.FileName,
                        FileExtension = fileExtension,
                        FilePath = relativePath,
                        FileSizeBytes = file.Length,
                        ContentType = file.ContentType,
                        Description = dto.Description,
                        AttachmentDate = now,
                        UploadedBy = userId,
                        SapId = sapId.Value,
                        IsActive = true
                    };

                    attachments.Add(attachment);
                }

                await _context.DocumentAttachments.AddRangeAsync(attachments);
                await _context.SaveChangesAsync();

                var result = attachments.Select(MapToDto).ToList();

                _logger.LogInformation(
                    "Uploaded {Count} attachments for {DocumentType} #{DocumentId}",
                    attachments.Count,
                    dto.DocumentType,
                    dto.DocumentId);

                return GeneralResponse<List<DocumentAttachmentDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading files for document {DocumentType} #{DocumentId}", dto.DocumentType,
                    dto.DocumentId);

                // Cleanup physical files لو حصل failure
                foreach (var path in savedPhysicalPaths)
                {
                    try
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to cleanup uploaded file: {Path}", path);
                    }
                }

                return GeneralResponse<List<DocumentAttachmentDto>>.FailResponse("Failed to upload document attachments");
            }
        }


       
        public async Task<GeneralResponse<List<NameStatus>>> GetDocumentStatus()
        {
            var statuses = Enum.GetValues(typeof(ProcessType))
                .Cast<ProcessType>()
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
      
        public async Task<GeneralResponse<List<DocumentAttachmentDto>>> GetDocumentsByTypeAndIdAsync(
           
            string documentType,
            int documentId)
        {
            try
            {
                var sapId = await sapCache.Get();

                if (!Enum.TryParse<ProcessType>(documentType, true, out var processType))
                {
                    throw new Exception($"Invalid document type: {documentType}");
                }

                var attachments = await _context.DocumentAttachments
                    .Where(x => x.SapId == sapId
                             && x.DocumentType == processType
                             && x.DocumentId == documentId
                             && x.IsActive)
                    .OrderByDescending(x => x.AttachmentDate)
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
                //  attachment.IsActive = false;

                //hard delete
                _context.DocumentAttachments.Remove(attachment);
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
                DocumentName = ((ProcessType)attachment.DocumentType).ToString(),
                DocumentId = attachment.DocumentId,
                 
                FilePath = attachment.FilePath,
                SourcePath = attachment.SourcePath,
                FullPath = $"{attachment.SourcePath}/{attachment.FilePath}",
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                FileExtension = attachment.FileExtension,
                FileSizeBytes = attachment.FileSizeBytes,
                FileSizeFormatted = FormatFileSize(attachment.FileSizeBytes),
                ContentType = attachment.ContentType,
                Description = attachment.Description,
                AttachmentDate = attachment.AttachmentDate,
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
