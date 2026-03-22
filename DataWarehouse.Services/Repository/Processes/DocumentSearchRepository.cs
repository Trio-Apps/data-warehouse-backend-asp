using DataWarehouse.Core.DTOs.Based;
using DataWarehouse.Core.Interfaces.Processes;
using DataWarehouse.Domain.Context;
using Google;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Services.Repository.Processes
{
    public class DocumentSearchRepository : IDocumentSearchRepository
    {
        private readonly DataWarehouseDbContext _context;

        public DocumentSearchRepository(DataWarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DocumentSearchResultDto>> SearchAsync(
            SearchDocumentsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var queryText = (request.Query ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(queryText))
                throw new ArgumentException("Query is required.", nameof(request.Query));

            request.PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var requestedTypes = request.DocumentTypes?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeType)
                .ToHashSet()
                ?? new HashSet<string>();

            var searchAll = requestedTypes.Count == 0;
            var isNumeric = int.TryParse(queryText, out var parsedNumber);

            var allResults = new List<DocumentSearchResultDto>();

            if (searchAll || requestedTypes.Contains(NormalizeType("PurchaseOrder")))
            {
                allResults.AddRange(await SearchEntityAsync(
                    _context.PurchaseOrders,
                    x => new DocumentSearchResultDto
                    {
                        DocumentType = x.DocType ?? "PurchaseOrder",
                        EntityName = "PurchaseOrder",
                        Id = x.PurchaseOrderId,
                        DocNum = x.DocNum,
                        DocEntry = x.DocEntry,
                        Status = (int)x.Status,
                        CreatedAt = x.CreatedAt,
                        PostingDate = x.PostingDate,
                        DueDate = x.DueDate,
                        Comment = x.Comment,
                        ErrorMessage = x.ErrorMessage
                    },
                    queryText,
                    isNumeric,
                    parsedNumber,
                    cancellationToken));
            }

            if (searchAll ||
                requestedTypes.Contains(NormalizeType("DeliveryNote")) ||
                requestedTypes.Contains(NormalizeType("DeliveryNoteOrder")))
            {
                allResults.AddRange(await SearchEntityAsync(
                    _context.DeliveryNoteOrders,
                    x => new DocumentSearchResultDto
                    {
                        DocumentType = x.DocType ?? "DeliveryNote",
                        EntityName = "DeliveryNoteOrder",
                        Id = x.DeliveryNoteOrderId,
                        DocNum = x.DocNum,
                        DocEntry = x.DocEntry,
                        Status = (int)x.Status,
                        CreatedAt = x.CreatedAt,
                        PostingDate = x.PostingDate,
                        DueDate = x.DueDate,
                        Comment = x.Comment,
                        ErrorMessage = x.ErrorMessage
                    },
                    queryText,
                    isNumeric,
                    parsedNumber,
                    cancellationToken));
            }

            if (searchAll || requestedTypes.Contains(NormalizeType("ReceiptOrder")))
            {
                allResults.AddRange(await SearchEntityAsync(
                    _context.ReceiptPurchaseOrders,
                    x => new DocumentSearchResultDto
                    {
                        DocumentType = x.DocType ?? "ReceiptOrder",
                        EntityName = "ReceiptOrder",
                        Id = x.ReceiptPurchaseOrderId,
                        DocNum = x.DocNum,
                        DocEntry = x.DocEntry,
                        Status = (int)x.Status,
                        CreatedAt = x.CreatedAt,
                        PostingDate = x.PostingDate,
                        DueDate = x.DueDate,
                        Comment = x.Comment,
                        ErrorMessage = x.ErrorMessage
                    },
                    queryText,
                    isNumeric,
                    parsedNumber,
                    cancellationToken));
            }

            if (searchAll || requestedTypes.Contains(NormalizeType("GoodsReturnOrder")))
            {
                allResults.AddRange(await SearchEntityAsync(
                    _context.GoodsReturnOrders,
                    x => new DocumentSearchResultDto
                    {
                        DocumentType = x.DocType ?? "GoodsReturnOrder",
                        EntityName = "GoodsReturnOrder",
                        Id = x.GoodsReturnOrderId,
                        DocNum = x.DocNum,
                        DocEntry = x.DocEntry,
                        Status = (int)x.Status,
                        CreatedAt = x.CreatedAt,
                        PostingDate = x.PostingDate,
                        DueDate = x.DueDate,
                        Comment = x.Comment,
                        ErrorMessage = x.ErrorMessage
                    },
                    queryText,
                    isNumeric,
                    parsedNumber,
                    cancellationToken));
            }

            if (searchAll || requestedTypes.Contains(NormalizeType("ProductionOrder")))
            {
                allResults.AddRange(await SearchEntityAsync(
                    _context.ProductionOrders,
                    x => new DocumentSearchResultDto
                    {
                        // لو ProductionOrder الجدول بتاعه مفيهوش DocNum/DocEntry/DocType/ErrorMessage
                        // سيبهم null أو قيمة ثابتة
                        DocumentType = "ProductionOrder",
                        EntityName = "ProductionOrder",
                        Id = x.ProductionOrderId,
                        DocNum = null,
                        DocEntry = null,
                        Status = (int)x.Status,
                        CreatedAt = x.CreatedAt,
                        PostingDate = x.PostingDate,
                        DueDate = x.DueDate,
                      //  Comment = x.Comment,
                        ErrorMessage = null
                    },
                    queryText,
                    isNumeric,
                    parsedNumber,
                    cancellationToken));
            }

            var orderedResults = allResults
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .ToList();

            var totalCount = orderedResults.Count;

            var pagedItems = orderedResults
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<DocumentSearchResultDto>
            {
                Data = pagedItems,
                TotalRecords = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private async Task<List<DocumentSearchResultDto>> SearchEntityAsync<TEntity>(
            IQueryable<TEntity> source,
            Expression<Func<TEntity, DocumentSearchResultDto>> selector,
            string queryText,
            bool isNumeric,
            int parsedNumber,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            var projectedQuery = source
                .AsNoTracking()
                .Select(selector);

            if (isNumeric)
            {
                projectedQuery = projectedQuery.Where(x =>
                    x.Id == parsedNumber ||
                    x.DocNum == parsedNumber ||
                    x.DocEntry == parsedNumber);
            }
            else
            {
                var likePattern = $"%{queryText}%";

                projectedQuery = projectedQuery.Where(x =>
                    (x.DocumentType != null && EF.Functions.Like(x.DocumentType, likePattern)) ||
                    (x.EntityName != null && EF.Functions.Like(x.EntityName, likePattern)) ||
                    (x.Comment != null && EF.Functions.Like(x.Comment, likePattern)) ||
                    (x.ErrorMessage != null && EF.Functions.Like(x.ErrorMessage, likePattern)));
            }

            return await projectedQuery.ToListAsync(cancellationToken);
        }

        private static string NormalizeType(string type)
        {
            return string.Concat(type
                    .Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-'))
                .ToLowerInvariant();
        }
    }
  
}
