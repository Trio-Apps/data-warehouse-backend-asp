using DataWarehouse.Domain.Context;
using DataWarehouse.Domain.Entities.Processes;
using DataWarehouse.SAP.Interfaces.Based;
using DataWarehouse.SAP.Interfaces.Proccesses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static DataWarehouse.SAP.Repositories.Proccesses.SapDeliveryNoteService;

namespace DataWarehouse.SAP.Repositories.Proccesses
{
    public class SapAttachmentService : ISapAttachmentService
    {
        private readonly IBaseSap<SapAttachmentDto> _sap;
        private readonly ILogger<SapAttachmentService> _logger;
        private readonly DataWarehouseDbContext _context;

        public SapAttachmentService(
            IBaseSap<SapAttachmentDto> sap,
            DataWarehouseDbContext context,
            ILogger<SapAttachmentService> logger)
        {
            _sap = sap;
            _logger = logger;
            _context = context;
        }

        public async Task<int?> CreateAttachmentEntryAsync(
            int sapId,
            List<DocumentAttachment> attachments)
        {
            try
            {
                if (attachments == null || attachments.Count == 0)
                    return null;

                var dto = new SapAttachmentDto();

                foreach (var file in attachments)
                {
                    if (string.IsNullOrWhiteSpace(file.FileName))
                        throw new InvalidOperationException("FileName is missing for attachment.");

                    if (string.IsNullOrWhiteSpace(file.FileExtension))
                        throw new InvalidOperationException("FileExtension is missing.");

                    dto.Attachments2_Lines.Add(new SapAttachmentLineDto
                    {
                        SourcePath = file.SourcePath,
                        FileName = Path.GetFileNameWithoutExtension(file.FileName),
                        FileExtension = file.FileExtension.Replace(".", ""),
                        AttachmentDate = ConvertToSapDateFormat(file.AttachmentDate)
                    });
                }

                var url = "Attachments2";

                var responseJson = await _sap.AddSapAsync(sapId, url, dto);

                var res = JsonSerializer.Deserialize<SapAttachmentResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (res == null)
                    throw new Exception("SAP did not return AttachmentEntry.");

                _logger.LogInformation(
                    "SAP AttachmentEntry created successfully. AttachmentEntry={Entry}",
                    res.AbsoluteEntry);

                return res.AbsoluteEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SAP attachment entry");
                throw;
            }
        }

        private string ConvertToSapDateFormat(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public class SapAttachmentDto
        {
            public List<SapAttachmentLineDto> Attachments2_Lines { get; set; } = new();
        }

        public class SapAttachmentLineDto
        {
            public string SourcePath { get; set; }
            public string FileName { get; set; }
            public string FileExtension { get; set; }
            public string AttachmentDate { get; set; }
        }
        public class SapAttachmentResponse
        {
            public int AbsoluteEntry { get; set; }
        }
    }
}
