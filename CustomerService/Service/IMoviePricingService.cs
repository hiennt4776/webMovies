using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Service
{
    public interface IMoviePricingService
    {
        Task<MovieAccessDTO> GetMovieAccessAsync(int movieId);
        Task<bool> HasActivePackageAsync(int userId);
        Task<List<MoviePricingDTO>> GetRentPricesAsync(int movieId);
        Task<MoviePricingDTO?> GetBuyPriceAsync(int movieId);
    }
    public class MoviePricingService : IMoviePricingService
    {
        private readonly dbMoviesContext _context;
        private readonly IMoviePricingRepository _moviePricingRepository;
        private readonly IMovieRepository _moviesRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MoviePricingService(dbMoviesContext context, IMovieRepository moviesRepository, IMoviePricingRepository moviePricingRepository,
                           IAuthService authService,
           IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _moviesRepository = moviesRepository;
            _moviePricingRepository = moviePricingRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> HasActivePackageAsync(int userId)
        {
            return await _context.InvoiceDetails
                .AnyAsync(d => d.PackageId != null
                && d.PackageStart <= DateTime.Now
                && d.PackageEnd >= DateTime.Now
                && d.IsDeleted == false
               && d.Invoice.UserCustomerId == userId
               && d.Invoice.PaymentStatus == "Paid"
               && d.Invoice.IsDeleted == false
                );

        }

        public async Task<bool> HasMovieAccessAsync(int userId, int movieId)
        {
            return await _context.InvoiceDetails.AnyAsync(d =>
                d.MovieId == movieId &&
                d.AccessStart <= DateTime.Now &&
                (d.AccessEnd == null || d.AccessEnd >= DateTime.Now) &&
               d.IsDeleted == false &&
                d.Invoice.IsDeleted == false &&
                d.Invoice.UserCustomerId == userId &&
                d.Invoice.PaymentStatus == "Paid");
        }


        public async Task<List<MoviePricing>> GetActiveAsync(int movieId)
        {
            return await _context.MoviePricings
                .Where(x =>
                    x.MovieId == movieId &&
                    x.IsActive == true &&
                    x.IsDeleted == false &&
                    x.StartDate <= DateTime.Now &&
                    (x.EndDate == null || x.EndDate >= DateTime.Now))
                .ToListAsync();
        }

        public async Task<MovieAccessDTO> GetMovieAccessAsync(int movieId)
        {

            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);


            var movie = await _context.MoviePricings.FindAsync(movieId)

                ?? throw new Exception("Movie not found");

            // 🎬 FREE
            if (movie.IsPaid == false)
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsPaid = false,
                    CanWatch = true
                };
            }



            // 📦 Subscription
            if (await HasActiveSubscriptionAsync(userId))
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsPaid = true,
                    CanWatch = true
                };
            }

            // 🧾 Purchased Rental
            if (await HasMovieAccessAsync(userId, movieId))
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsPaid = true,
                    CanWatch = true
                };
            }

        

            // ❌ Không có quyền → show giá
            var pricing = await GetActiveAsync(movieId);

            return new MovieAccessDTO
            {
                MovieId = movieId,
                IsPaid = true,
                CanWatch = false,

                ShowBuy = pricing.Any(x => x.PricingType == "Buy"),
                ShowRent = pricing.Any(x => x.PricingType == "Rent"),

                BuyPrice = pricing.FirstOrDefault(x => x.PricingType == "Buy")?.Price,
                RentPrice = pricing.FirstOrDefault(x => x.PricingType == "Rent")?.Price,
                RentalDurationDays = pricing.FirstOrDefault(x => x.PricingType == "Rent")?.RentalDurationDays
            };
        }

        private async Task<bool> HasActiveSubscriptionAsync(int userId)
        {
            return await _context.InvoiceDetails
                  .AnyAsync(d => d.PackageId != null
                  && d.PackageStart <= DateTime.Now
                  && d.PackageEnd >= DateTime.Now
                  && d.IsDeleted == false
                 && d.Invoice.UserCustomerId == userId
                 && d.Invoice.PaymentStatus == "Paid"
                 && d.Invoice.IsDeleted == false
                  );
        }

        public async Task<List<MoviePricingDTO>> GetRentPricesAsync(int movieId)
        {
            return await _context.MoviePricings
                .Where(x => x.MovieId == movieId
                         && x.PricingType == "RENT"
                         && x.IsActive == true
                         && x.IsDeleted == false)
                .Select(x => new MoviePricingDTO
                {
                    Id = x.Id,
                    MovieId = x.MovieId,
                    PricingType = x.PricingType,
                    Price = x.Price,
                    RentalDurationDays = x.RentalDurationDays
                })
                .ToListAsync();
        }

        public async Task<MoviePricingDTO?> GetBuyPriceAsync(int movieId)
        {
            return await _context.MoviePricings
                .Where(x => x.MovieId == movieId
                         && x.PricingType == "BUY"
                       && x.IsActive == true
                         && x.IsDeleted == false)
                .Select(x => new MoviePricingDTO
                {
                    Id = x.Id,
                    MovieId = x.MovieId,
                    PricingType = x.PricingType,
                    Price = x.Price
                })
                .FirstOrDefaultAsync();
        }

       

    }
}
