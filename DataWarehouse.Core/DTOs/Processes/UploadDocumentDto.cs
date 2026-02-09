using DataWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataWarehouse.Core.DTOs.Processes
{
    public class UploadDocumentDto
    {
        [Required]
        public DocumentType DocumentType { get; set; }

        [Required]
        public int DocumentId { get; set; }

        // File validation is done manually in the controller
        // [Required] attribute doesn't work well with IFormFile in [FromForm]
        public IFormFile? File { get; set; }

        public string? Description { get; set; }
    }
}
