
using dbMovies.Models;
using helperMovies.Constants;
using helperMovies.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace CustomerService.Service
{
    public interface ICustomerService
    {

        Task<CustomerDTO> GetProfileAsync(string username);

        Task<List<InvoiceDTO>> GetInvoicesAsync();
        Task<List<FavoriteMovieDTO>> GetFavoriteMoviesAsync();

    }
    public class CustomerDetailService : ICustomerService
    {
        private readonly dbMoviesContext _context;
        public readonly IUserCustomerRepository _userCustomerRepository;
        public readonly ICustomerRepository _CustomerRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerDetailService(dbMoviesContext context,
            IUserCustomerRepository userCustomerRepository,
            ICustomerRepository CustomerRepository,
              IAuthService authService,
            IUnitOfWork dbu,
            IConfiguration config,
            JwtAuthService jwtAuthService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userCustomerRepository = userCustomerRepository;
            _CustomerRepository = CustomerRepository;
            _authService = authService;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<CustomerDTO> GetProfileAsync(string username)
        {
            var user = await _context.UserCustomers
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.Customer == null) return null;

            // Map entity -> DTO
            return new CustomerDTO //nó bị lỗi ở admin hay user vậy bạn 
            //moi lan login se bao
            //Path: /api/UserCustomer/profile
            //Authorization: No token
            {
                FullName = user.Customer.FullName,
                Email = user.Customer.Email,
                Phone = user.Customer.Phone,
                Address = user.Customer.Address,
                DateOfBirth = user.Customer.DateOfBirth
            };


        }


        public async Task<List<InvoiceDTO>> GetInvoicesAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            //var invoices = await _context.Invoices
            //    .Where(x => x.UserCustomerId == userId && x.IsDeleted == false)
            //    .OrderByDescending(x => x.InvoiceDate)
            //    .ToListAsync();

            //var result = new List<InvoiceDTO>();

            //foreach (var invoice in invoices)
            //{
            //    var details = await _context.InvoiceDetails
            //        .Where(d => d.InvoiceId == invoice.Id && d.IsDeleted==false)
            //        .Select(d => new InvoiceDetailDTO
            //        {
            //            MovieId = d.MovieId,
            //            PricingType = d.PricingType,
            //            UnitPrice = d.UnitPrice,

            //        })
            //        .ToListAsync();

            //    result.Add(new InvoiceDTO
            //    {
            //        Id = invoice.Id,
            //        InvoiceNumber = invoice.InvoiceNumber,
            //        InvoiceDate = invoice.InvoiceDate,
            //        TotalAmount = invoice.TotalAmount,
            //        PaymentStatus = invoice.PaymentStatus,
            //        PaymentMethod = invoice.PaymentMethod,
            //        Notes = invoice.Notes,
            //        IsDeleted = invoice.IsDeleted,
            //        Reason = invoice.Reason,
            //        InvoiceDetail = invoice.InvoiceDetails

            //        .Select(d => new InvoiceDetailDTO
            //        {
            //            Id = d.Id,



            //            MovieId = d.MovieId,
            //            MovieTitle = d.Movie?.Title,

            //            PackageId = d.PackageId,
            //            PackageName = d.Package?.Name,

            //            PricingType = d.PricingType,
            //            UnitPrice = d.UnitPrice,

            //            StartDate = d.MovieId != null ? d.AccessStart : d.PackageStart,
            //            EndDate = d.MovieId != null ? d.AccessEnd : d.PackageEnd
            //        })
            //        .ToList()
            //    });
            //}

            //return result;

         
                        var data = await (
                                 from i in _context.Invoices
                                 join d in _context.InvoiceDetails
                                     on i.Id equals d.InvoiceId into g
                                 where i.UserCustomerId == userId
                                       && i.IsDeleted == false
                                 select new
                                 {
                                     Invoice = i,
                                     Details = g.Where(x => x.IsDeleted == false || x.IsDeleted == null)
                                 }
                             )
                             .OrderByDescending(i => i.Invoice.InvoiceDate)
                             .ToListAsync();

            var result = data.Select(x => new InvoiceDTO
            {
                Id = x.Invoice.Id,
                InvoiceNumber = x.Invoice.InvoiceNumber,
                InvoiceDate = x.Invoice.InvoiceDate,
                TotalAmount = x.Invoice.TotalAmount,
                PaymentStatus = x.Invoice.PaymentStatus,
                InvoiceDetail = x.Details.Select(d => new InvoiceDetailDTO
                {
                    MovieId = d.MovieId,
                    MovieTitle = d.MovieId != null
                        ? _context.Movies
                            .Where(m => m.Id == d.MovieId)
                            .Select(m => m.Title)
                            .FirstOrDefault()
                        : null,
                    PackageId = d.PackageId,
                    PricingType = d.PricingType,
                    PackageName = d.PackageId != null
                        ? _context.SubscriptionPackages
                            .Where(p => p.Id == d.PackageId)
                            .Select(p => p.Name)
                            .FirstOrDefault()
                        : null,
                    UnitPrice = d.UnitPrice,
                    StartDate = d.MovieId != null ? d.AccessStart : d.PackageStart,
                    EndDate = d.MovieId != null ? d.AccessEnd : d.PackageEnd
                }).ToList()
            }).ToList();

            return result;

        }

        public async Task<List<FavoriteMovieDTO>> GetFavoriteMoviesAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            return await _context.FavoriteMovies
                .Where(x => x.UserCustomerId == userId)
                .Select(x => new FavoriteMovieDTO
                {
                    MovieId = x.MovieId,
                    MovieName = _context.Movies
                        .Where(m => m.Id == x.MovieId)
                        .Select(m => m.Title)
                        .FirstOrDefault()!,
                    AddedDate = x.AddedDate
                })
                .ToListAsync();
        }
    }
}
