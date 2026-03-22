using DataWarehouse.Core.Interfaces.Processes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Api.Controllers.admin.Processes
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchScanFlexibilityController : ControllerBase
    {
        private readonly IDocumentSearchRepository _documentSearchRepository;

        public SearchScanFlexibilityController(IDocumentSearchRepository documentSearchRepository)
        {
            _documentSearchRepository = documentSearchRepository;
        }

        [HttpGet("scan")]
        public async Task<IActionResult> Scan(
            [FromQuery] string query,
            [FromQuery] List<string>? documentTypes,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new SearchDocumentsRequest
            {
                Query = query,
                DocumentTypes = documentTypes ?? new List<string>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _documentSearchRepository.SearchAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(
            [FromBody] SearchDocumentsRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _documentSearchRepository.SearchAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
