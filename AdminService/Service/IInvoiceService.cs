using AdminService.Services;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
namespace AdminService.Service
{
    public interface IInvoiceService
    {
        Task<PagedResult<InvoiceDTO>> GetInvoicesAsync(InvoiceQuery q);

        Task CancelInvoiceAsync(int invoiceId, string reason);
        Task<InvoiceDTO?> GetDetailAsync(int id);
    }
    public class InvoiceService : IInvoiceService
    {
        private readonly dbMoviesContext _context;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public InvoiceService(dbMoviesContext context,
            IInvoiceRepository invoiceRepository,
                        IAuthService authService,
        IUnitOfWork dbu,
        JwtAuthService jwtAuthService,
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _invoiceRepository = invoiceRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<PagedResult<InvoiceDTO>> GetInvoicesAsync(
            InvoiceQuery q)
        {
            var query = _context.Invoices.AsNoTracking();

            // 🗑 FILTER DELETE
            if (q.IsDeleted.HasValue)
            {
                query = query.Where(x => x.IsDeleted == q.IsDeleted);
            }

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(q.Keyword))
            {
                query = query.Where(x =>
                    x.InvoiceNumber.Contains(q.Keyword));
            }

            // ↕ SORT
            query = q.SortBy switch
            {
                "TotalAmount" => q.SortDesc
                    ? query.OrderByDescending(x => x.TotalAmount)
                    : query.OrderBy(x => x.TotalAmount),

                _ => q.SortDesc
                    ? query.OrderByDescending(x => x.InvoiceDate)
                    : query.OrderBy(x => x.InvoiceDate)
            };

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(x => new InvoiceDTO
                {
                    Id = x.Id,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceDate = x.InvoiceDate,
                    TotalAmount = x.TotalAmount,
                    PaymentStatus = x.PaymentStatus,
                    PaymentMethod = x.PaymentMethod,
                    IsDeleted = x.IsDeleted   
                })
                .ToListAsync();

            return new PagedResult<InvoiceDTO>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = q.Page,
                PageSize = q.PageSize
            };
        }

        public async Task CancelInvoiceAsync(int invoiceId, string reason)
        {
            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var invoice = await _context.Invoices
                .Include(x => x.InvoiceDetails)
                .FirstOrDefaultAsync(x => x.Id == invoiceId);

            if (invoice == null)
                throw new Exception("Invoice not found");

            invoice.IsDeleted = true;
            invoice.Reason = reason;
            invoice.UpdatedDate = DateTime.Now;

            foreach (var detail in invoice.InvoiceDetails)
            {
                detail.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<InvoiceDTO?> GetDetailAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(x => x.UserCustomer)
                .ThenInclude(x => x.Customer)
                .Include(x => x.InvoiceDetails)
                    .ThenInclude(d => d.Movie)
                .Include(x => x.InvoiceDetails)
                    .ThenInclude(d => d.Package)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (invoice == null) return null;

            return new InvoiceDTO
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                UserCustomerId = invoice.UserCustomerId,
                CustomerName = invoice.UserCustomer.Customer.FullName,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                PaymentMethod = invoice.PaymentMethod,
                IsDeleted = invoice.IsDeleted,
                Reason = invoice.Reason,
                InvoiceDetail = invoice.InvoiceDetails.Select(d => new InvoiceDetailDTO
                {
                    Id = d.Id,
                    MovieId = d.MovieId,
                    MovieTitle = d.MovieId != null ? d.Movie.Title : null,
                    PackageId = d.PackageId ,
                    PackageName = d.Package != null ? d.Package.Name : null,
                    PricingType = d.PricingType,
                    UnitPrice = d.UnitPrice,
                    StartDate = d.MovieId != null ? d.AccessStart : d.PackageStart,
                    EndDate = d.MovieId != null ? d.AccessEnd : d.PackageEnd
                }).ToList()
            };
        }

    }

}
