using DataWarehouse.Core.DTOs;
using DataWarehouse.Core.DTOs.Processes;
using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.Interfaces.Processes
{
    public interface IDocumentAttachmentRepository
    {
        Task<GeneralResponse<DocumentAttachmentDto>> UploadDocumentAsync(
      
       UploadDocumentDto dto,
       string userId);

        Task<GeneralResponse<List<DocumentAttachmentDto>>> UploadMultipleDocumentsAsync(
            
            UploadMultipleDocumentsDto dto,
            string userId);

        Task<GeneralResponse<List<DocumentAttachmentDto>>> GetDocumentsByTypeAndIdAsync( 
            DocumentType documentType,
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
