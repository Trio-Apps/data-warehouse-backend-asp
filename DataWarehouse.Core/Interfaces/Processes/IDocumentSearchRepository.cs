using DataWarehouse.Core.DTOs.Based;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes
{
    public interface IDocumentSearchRepository
    {
        Task<PagedResult<DocumentSearchResultDto>> SearchAsync(
            SearchDocumentsRequest request,
            CancellationToken cancellationToken = default);
    }

    public class SearchDocumentsRequest
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        public List<string> DocumentTypes { get; set; } = new();

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DocumentSearchResultDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;

        public int Id { get; set; }
        public int? DocNum { get; set; }
        public int? DocEntry { get; set; }

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DueDate { get; set; }

        public string? Comment { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
