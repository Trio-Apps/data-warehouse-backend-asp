using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Domain.Enums;
using DataWarehouse.Domain.Enums.Approval;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes
{
    public interface IDocumentAttachmentRepository
    {

        Task<GeneralResponse<List<DocumentAttachmentDto>>> UploadFilesForDocumentAsync(

    string userId,
     UploadDocumentDto dto);



        Task<GeneralResponse<List<DocumentAttachmentDto>>> GetDocumentsByTypeAndIdAsync(
            string documentType,
            int documentId);

        Task<GeneralResponse<byte[]>> DownloadDocumentAsync(
            int documentAttachmentId);

        Task<GeneralResponse<bool>> DeleteDocumentAsync(
            
            int documentAttachmentId);

        Task<DocumentAttachmentDto?> GetDocumentAttachmentByIdAsync(
            
            int documentAttachmentId);

        Task<GeneralResponse<List<NameStatus>>> GetDocumentStatus();
    }
}
