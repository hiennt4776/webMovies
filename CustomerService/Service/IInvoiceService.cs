using Confluent.Kafka;
using dbMovies.Models;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Service
{

    public interface IInvoiceService
    {


    Task<int> PurchaseMovieAsync(
            int userCustomerId,
            int movieId,
            string pricingType);

    }
    public class InvoiceService : IInvoiceService
    {
        private readonly dbMoviesContext _context;
        public readonly IUserCustomerRepository _userCustomerRepository;
        public readonly ICustomerRepository _CustomerRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvoiceService(
            dbMoviesContext context,
            IUserCustomerRepository userCustomerRepository,
            ICustomerRepository CustomerRepository,
            IUnitOfWork dbu,
                IConfiguration config,
          JwtAuthService jwtAuthService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userCustomerRepository = userCustomerRepository;
            _CustomerRepository = CustomerRepository;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<int> PurchaseMovieAsync(
            int userCustomerId,
            int movieId,
            string pricingType)
        {
            // 1️⃣ Lấy giá phim
            var pricing = await _context.MoviePricings.FirstOrDefaultAsync(x =>
                    x.MovieId == movieId &&
                    x.PricingType == pricingType &&
                    x.IsActive == true &&
                    x.IsDeleted == false);


            if (pricing == null)
                throw new Exception("Phim chưa có giá hoặc không khả dụng");

            // 2️⃣ Tính thời gian truy cập
            DateTime? accessStart = DateTime.Now;
            DateTime? accessEnd = null;

            if (pricingType == PricingType.Rent)
            {
                accessEnd = accessStart.Value.AddDays(pricing.RentalDurationDays ?? 0);
            }

            // 3️⃣ Tạo hóa đơn
            var invoice = new Invoice
            {
                UserCustomerId = userCustomerId,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                InvoiceDate = DateTime.Now,
                TotalAmount = pricing.Price,
                PaymentStatus = pricingType, // giả sử đã thanh toán
                PaymentMethod = "Wallet",
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // 4️⃣ Tạo chi tiết hóa đơn
            var invoiceDetail = new InvoiceDetail
            {
                InvoiceId = invoice.Id,
                MovieId = movieId,
                PricingType = pricingType,
                UnitPrice = pricing.Price,
                AccessStart = accessStart,
                AccessEnd = accessEnd,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.InvoiceDetails.Add(invoiceDetail);

            // 5️⃣ Đánh dấu pricing đã được mua (nếu cần)
            pricing.IsPaid = true;
            pricing.StartDate = accessStart;
            pricing.EndDate = accessEnd;

            await _context.SaveChangesAsync();

            return invoice.Id;
        }

   
    }

}
