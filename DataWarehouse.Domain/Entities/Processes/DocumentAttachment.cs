using DataWarehouse.Domain.Entities.AllinAll;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Processes
{
    public class DocumentAttachment
    {
        public int DocumentAttachmentId { get; set; }

        // Reference للـ Document
        public DocumentType DocumentType { get; set; }
        public int DocumentId { get; set; } // Foreign key للـ Purchase/Sales/Production

        // File Info
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; }

        // Metadata
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Relations
        public int SapId { get; set; }
        public Sap Sap { get; set; }
    }
}
