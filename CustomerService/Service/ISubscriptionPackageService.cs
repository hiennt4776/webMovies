
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
using helperMovies.constMovies;


namespace CustomerService.Service
{
    public interface ISubscriptionPackageService
    {
        Task<List<SubscriptionPackageDTO>> GetAllAsync();
        Task<PagedResult<SubscriptionPackageDTO>> GetAllPagedAsync(string? search, int page, int pageSize);
        Task<SubscriptionPackageDTO?> GetByIdAsync(int id);
        Task<int> PurchaseAsync(int packageId);
        Task SimulatePaymentAsync(int invoiceId, bool success);

    }

    public class SubscriptionPackageService : ISubscriptionPackageService
    {
        private readonly dbMoviesContext _context;
        private readonly ISubscriptionPackageRepository _subscriptionPackageRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SubscriptionPackageService(
            dbMoviesContext context,
            ISubscriptionPackageRepository subscriptionPackageRepository,
            IAuthService authService,
            IUnitOfWork dbu,
            IConfiguration config,
            JwtAuthService jwtAuthService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _subscriptionPackageRepository = subscriptionPackageRepository;
            _authService = authService;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<List<SubscriptionPackageDTO>> GetAllAsync()
        {
            return await _context.SubscriptionPackages
                .Where(x => x.IsActive == true && x.IsDeleted == false)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Description = x.Description,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<PagedResult<SubscriptionPackageDTO>> GetAllPagedAsync(string? search, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.SubscriptionPackages.AsQueryable()
                .Where(x => x.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Description = x.Description,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResult<SubscriptionPackageDTO>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<SubscriptionPackageDTO?> GetByIdAsync(int id)
        {
            return await _context.SubscriptionPackages
                .Where(x => x.Id == id && x.IsDeleted == false)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Description = x.Description,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var now = DateTime.Now;
            var prefix = now.ToString("yyMM"); // VD: 2512

            // Lấy invoice lớn nhất trong tháng hiện tại
            var lastInvoice = await _context.Invoices
                .Where(x => x.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(x => x.InvoiceNumber)
                .Select(x => x.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextSequence = 1;

            if (!string.IsNullOrEmpty(lastInvoice))
            {
                // Cắt 5 số cuối
                var lastSeq = int.Parse(lastInvoice.Substring(4, 5));
                nextSequence = lastSeq + 1;
            }

            return $"{prefix}{nextSequence:D5}";
        }


        public async Task<int> PurchaseAsync(int packageId)
        {

                        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
                        int userId = _authService.GetUserIdFromToken(httpContext);

            //int userId = _authService.GetUserId(User);

            //await _service.PurchaseAsync(packageId, userId);
            int k = 1;

            int k2 = packageId;
            //int userId = _authService.GetUserIdFromToken();

            var package = await _context.SubscriptionPackages
                .FirstOrDefaultAsync(x =>
                    x.Id == packageId &&
                    x.IsActive == true &&
                    x.IsDeleted == false);

            if (package == null)
                throw new Exception("Gói không tồn tại");

            var now = DateTime.UtcNow;

            var current = await _context.UserCustomers
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync();


            // 1️⃣ Gói hiện tại từ InvoiceDetails
            var currentPackage = await _context.InvoiceDetails
                .Where(d =>
                    d.Invoice.UserCustomerId == userId &&
                    d.PricingType == PricingType.PACKAGE &&
                    d.IsDeleted == false &&
                    d.PackageEnd > now)
                .OrderByDescending(d => d.PackageEnd)
                .FirstOrDefaultAsync();

            var startDate = currentPackage != null
                ? currentPackage.PackageEnd!.Value
                : now;

            var endDate = startDate.AddMonths((int)package.DurationMonths);

            using var transaction = await _context.Database.BeginTransactionAsync();

            // 2️⃣ Invoice
            try
            {
                var invoice = new Invoice
                {
                    UserCustomerId = userId,
                    InvoiceNumber = await GenerateInvoiceNumberAsync(),
                    InvoiceDate = now,
                    TotalAmount = package.Price,
                    PaymentStatus = PaymentStatus.PAID,
                    PaymentMethod = PaymentMethod.CASH,
                    CreatedDate = DateTime.Now,
                     IsDeleted = false
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // 3️⃣ InvoiceDetail (PACKAGE)
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    PackageId = package.Id,
                    PricingType = PricingType.PACKAGE,
                    UnitPrice = package.Price,
                    PackageStart = startDate,
                    PackageEnd = endDate,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };
                _context.InvoiceDetails.Add(detail);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return invoice.Id;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }


        }

        public async Task SimulatePaymentAsync(int invoiceId, bool success)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(x => x.Id == invoiceId && x.IsDeleted == false);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice không tồn tại");

            invoice.PaymentStatus = success ? "Paid" : "Failed";

            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

        }

    }
}