using AdminService.Services;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Service
{
    public interface IMoviePricingService
    {
        Task<IEnumerable<MoviePricingDTO>> GetByMovieAsync(int movieId);
        Task<MoviePricingDTO> GetActivePriceAsync(int movieId, DateTime utcNow);
        Task<MoviePricingDTO> GetByIdAsync(int id);
        Task<MoviePricingDTO> CreateAsync(CreateUpdateMoviePricingDTO dto);
        Task<MoviePricingDTO> UpdateAsync(int id, CreateUpdateMoviePricingDTO dto);
        Task<bool> SoftDeleteAsync(int id);
        // Rent/Buy helper
        Task<DateTime?> RentAsync(int pricingId); // returns expiry UTC if rent succeeded
    }
    public class MoviePricingService : IMoviePricingService
    {
        private readonly dbMoviesContext _context;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MoviePricingService(dbMoviesContext context, IMovieRepository movieRepository,
                           IAuthService authService,
           IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _movieRepository = movieRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MoviePricingDTO> CreateAsync(CreateUpdateMoviePricingDTO dto)
        {

            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);



            var entity = new MoviePricing
            {
                MovieId = dto.MovieId,
                IsPaid = dto.IsPaid,
                PricingType = dto.PricingType,
                Price = dto.Price,
                RentalDurationDays = dto.RentalDurationDays,
                StartDate = dto.StartDate?.ToUniversalTime(),
                EndDate = dto.EndDate?.ToUniversalTime(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false,
            };

            _context.MoviePricings.Add(entity);
            await _context.SaveChangesAsync();

            return new MoviePricingDTO
            {
                Id = entity.Id,
                MovieId = entity.MovieId,
                IsPaid = entity.IsPaid,
                PricingType = entity.PricingType,
                Price = entity.Price,
                RentalDurationDays = entity.RentalDurationDays,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive
            };
        }

        public async Task<MoviePricingDTO> GetActivePriceAsync(int movieId, DateTime utcNow)
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);


            var p = await _context.MoviePricings
                .Where(x => x.MovieId == movieId && x.IsActive == true && x.IsDeleted == false)
                          
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();

            return p == null ? null : new MoviePricingDTO
            {
                Id = p.Id,
                MovieId = p.MovieId,
                IsPaid = p.IsPaid,
                PricingType = p.PricingType,
                Price = p.Price,
                RentalDurationDays = p.RentalDurationDays,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            };
        }

        public async Task<IEnumerable<MoviePricingDTO>> GetByMovieAsync(int movieId)
        {
            return await _context.MoviePricings
                .Where(x => x.MovieId == movieId && x.IsDeleted == false)
                .OrderByDescending(x => x.StartDate)
                 .Select(x => new MoviePricingDTO
                 {
                     Id = x.Id,
                     MovieId = x.MovieId,
                     Price = x.Price,
                     PricingType = x.PricingType,
                     StartDate = x.StartDate,
                     EndDate = x.EndDate,
                     IsActive = x.IsActive,
                     RentalDurationDays = x.RentalDurationDays
                 })
                .ToListAsync();
        }

        public async Task<MoviePricingDTO> GetByIdAsync(int id)
        {
            var p = await _context.MoviePricings.FindAsync(id);
            return p == null || p.IsDeleted == true? null : new MoviePricingDTO
            {
                Id = p.Id,
                MovieId = p.MovieId,
                IsPaid = p.IsPaid,
                PricingType = p.PricingType,
                Price = p.Price,
                RentalDurationDays = p.RentalDurationDays,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            };
        }

        public async Task<MoviePricingDTO> UpdateAsync(int id, CreateUpdateMoviePricingDTO dto)
        {

            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);



            var p = await _context.MoviePricings.FindAsync(id);
            if (p == null || p.IsDeleted == true) return null;

            p.IsPaid = dto.IsPaid;
            p.PricingType = dto.PricingType;
            p.Price = dto.Price;
            p.RentalDurationDays = dto.RentalDurationDays;
            p.StartDate = dto.StartDate?.ToUniversalTime();
            p.EndDate = dto.EndDate?.ToUniversalTime();

            p.IsActive = dto.IsActive;
            p.UpdatedDate = DateTime.Now;
            p.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return new MoviePricingDTO
            {
                Id = p.Id,
                MovieId = p.MovieId,
                IsPaid = p.IsPaid,
                PricingType = p.PricingType,
                Price = p.Price,
                RentalDurationDays = p.RentalDurationDays,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            }; ;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var p = await _context.MoviePricings.FindAsync(id);
            if (p == null || p.IsDeleted == true) return false;
            p.IsDeleted = true;
            p.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DateTime?> RentAsync(int pricingId)
        {
            var p = await _context.MoviePricings.FindAsync(pricingId);
            if (p == null || p.IsDeleted == true || p.IsActive == false) return null;
            if (!string.Equals(p.PricingType, "Rent", StringComparison.OrdinalIgnoreCase))
                return null;
            if (p.RentalDurationDays == null || p.RentalDurationDays <= 0)
                return null;

            // In a real app: create a Rental/Invoice record, charge user, etc.
            var now = DateTime.Now;
            var expiry = now.AddDays(p.RentalDurationDays.Value);
            // here you might persist a rental history record. We just return expiry.
            return expiry;
        }

        //public MoviePricingDTO Map(MoviePricing p)
        //{
        //    return new MoviePricingDTO
        //    {
        //        Id = p.Id,
        //        MovieId = p.MovieId,
        //        IsPaid = p.IsPaid,
        //        PricingType = p.PricingType,
        //        Price = p.Price,
        //        RentalDurationDays = p.RentalDurationDays,
        //        StartDate = p.StartDate,
        //        EndDate = p.EndDate,
        //        IsActive = p.IsActive
        //    };
        //}
            
    }
}
