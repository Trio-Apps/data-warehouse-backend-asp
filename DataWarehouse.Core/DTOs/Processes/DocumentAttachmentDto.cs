using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs.Processes
{
    public class DocumentAttachmentDto
    {
        public int DocumentAttachmentId { get; set; }
        public ProcessType DocumentType { get; set; }
        public string? DocumentName { get; set; }

        public string SourcePath { get; set; }
        public string FilePath { get; set; }
        public string FullPath { get; set; }
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; }
        public string ContentType { get; set; }
        public string? Description { get; set; }
        public DateTime AttachmentDate { get; set; }
        public string UploadedBy { get; set; }
        public string DownloadUrl { get; set; }
    }
}
