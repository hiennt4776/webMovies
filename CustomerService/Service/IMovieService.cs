//using CustomerService.Utils;
using dbMovies.Models;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;



namespace CustomerService.Service
{

    
    public interface IMovieService
    {
        Task<List<MovieDTO>> GetRandomTop5FilmsAsync();  // đổi trả DTO
        Task<List<MovieDTO>> GetMoviesAsync();
        Task<MovieDTO> GetMovieByIdAsync(int movieId);
        Task<byte[]> GetMovieFileAsync(int movieId, string fileType);
        Task<PagedResult<MovieDTO>> SearchMoviesAsync(MovieQueryDTO query);
        Task<MovieAccessDTO> GetMovieAccessAsync(int movieId);
    }

    public class MovieService : IMovieService
    {

        private readonly dbMoviesContext _context;
        public IMovieRepository _moviesRepository;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieService(
            dbMoviesContext context, 
            IMovieRepository moviesRepository,
              IFileService fileService,
             IAuthService authService,
            IUnitOfWork dbu,
            IConfiguration config,
            JwtAuthService jwtAuthService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _moviesRepository = moviesRepository;
            _fileService = fileService; 
              _authService = authService;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedResult<MovieDTO>> SearchMoviesAsync(MovieQueryDTO query)
        {
            var q = _context.Movies
                .Include(m => m.Category)
                               .Where(m => m.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
                q = q.Where(m => m.Title.Contains(query.Keyword));

            if (query.CategoryId.HasValue)
                q = q.Where(m => m.CategoryId == query.CategoryId);

            // filter theo YEAR (DateOnly)
            if (query.ReleaseYear.HasValue)
                q = q.Where(m => m.ReleaseDate.Value.Year == query.ReleaseYear);

            var totalItems = await q.CountAsync();

            var items = await q
                .OrderBy(m => m.Title)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var dtos = items.Select(m => Map(m)).ToList();

            return new PagedResult<MovieDTO>
            {
                Items = dtos,
                TotalCount = totalItems,
                PageNumber = query.Page,
                PageSize = query.PageSize
            };
        }



        public async Task<List<MovieDTO>> GetRandomTop5FilmsAsync()
        {

            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            var k = await _context.Movies
                  .Where(m => m.IsDeleted == false
                      && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == FileTypeMovieConstant.POSTER))
                  .OrderBy(m => Guid.NewGuid())
                  .Take(5)
                  .Select(m => new MovieDTO
                  {
                      Id = m.Id,
                      Title = m.Title,
                      ReleaseDate = m.ReleaseDate,
                      // chỉ thêm /movies nếu đường dẫn trong DB KHÔNG chứa nó
                      FilePath = $"{baseUrl}/{m.MovieFiles
                          .Where(f => f.IsDeleted == false && f.FileType == FileTypeMovieConstant.POSTER)
                          .Select(f => f.FilePath.Replace("\\", "/"))
                          .FirstOrDefault()}"
                  })
                  .ToListAsync();

            return await _context.Movies
                  .Where(m => m.IsDeleted == false
                      && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == FileTypeMovieConstant.POSTER))
                  .OrderBy(m => Guid.NewGuid())
                  .Take(5)
                  .Select(m => new MovieDTO
                  {
                      Id = m.Id,
                      Title = m.Title,
                      ReleaseDate = m.ReleaseDate,
                      // chỉ thêm /movies nếu đường dẫn trong DB KHÔNG chứa nó
                      FilePath = $"{baseUrl}/{m.MovieFiles
                          .Where(f => f.IsDeleted == false && f.FileType == FileTypeMovieConstant.POSTER)
                          .Select(f => f.FilePath.Replace("\\", "/"))
                          .FirstOrDefault()}"
                  })
                  .ToListAsync();
        }

        public async Task<List<MovieDTO>> GetMoviesAsync()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieFiles)
                .Where(m => m.IsDeleted == false)
                .Select(m => new MovieDTO
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    HasTrailer = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.TRAILER && f.IsDeleted == false),
                    HasMovie = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.MOVIES && f.IsDeleted == false),
                    HasSubtitle = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.SUBTITLE && f.IsDeleted == false)
                })
                .ToListAsync();

            return movies;
        }

        public async Task<MovieDTO> GetMovieByIdAsync(int movieId)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieFiles)
                .Where(m => m.Id == movieId && m.IsDeleted == false)
                .Select(m => new MovieDTO
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    HasTrailer = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.TRAILER && f.IsDeleted == false),
                    HasMovie = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.MOVIES && f.IsDeleted == false),
                    HasSubtitle = m.MovieFiles.Any(f => f.FileType == FileTypeMovieConstant.SUBTITLE && f.IsDeleted == false)
                })
                .FirstOrDefaultAsync();

            return movie;
        }

        public async Task<byte[]> GetMovieFileAsync(int movieId, string fileType)
        {
            var file = await _context.MovieFiles
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.FileType.ToLower() == fileType.ToLower() && f.IsDeleted == false);

            if (file == null) return null;

            var bytes = _fileService.GetFileBytes(file.FilePath);
            return bytes;
        }

        public string GetFileUrl(int movieId, string fileType)
        {
            return $"api/movie/file/{movieId}/{fileType}";
        }


        private MovieDTO Map(Movie m) =>
          new MovieDTO
          {
              Id = m.Id,
              Title = m.Title,
              Director = m.Director ?? "",
              ReleaseDate = m.ReleaseDate,
              LanguageList = m.LanguageList ?? "",
              Country = m.Country ?? "",
              Status = m.Status ?? "",
              Budget = m.Budget,
              BoxOffice = m.BoxOffice,
              Files = m.MovieFiles?.Where(f => f.IsDeleted == false).Select(f => new MovieFileDTO
              {
                  Id = f.Id,
                  MovieId = f.MovieId,
                  FileType = f.FileType,
                  FileName = f.FileName,
                  FilePath = f.FilePath
              }).ToList() ?? new List<MovieFileDTO>()
          };

        ///
        public async Task<Movie?> GetByIdAsync(int id)
            => await _context.Movies.FindAsync(id);
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

        public async Task<bool> IsFreeMovieAsync(int movieId)
        {
            return await _context.MoviePricings.AnyAsync(x =>
                x.MovieId == movieId &&
                x.PricingType == PricingType.FREE &&
                x.IsActive == true &&
                x.IsDeleted == false &&
                x.StartDate <= DateTime.Now &&
                (x.EndDate == null || x.EndDate >= DateTime.Now));
        }

        public async Task<bool> HasActivePackageAsync(int userId)
        {
            return await _context.InvoiceDetails.AnyAsync(d =>
                d.PackageId != null &&
                d.PackageStart <= DateTime.Now &&
                d.PackageEnd >= DateTime.Now &&
                d.IsDeleted == false &&
                d.Invoice.IsDeleted == false &&
                d.Invoice.UserCustomerId == userId &&
                d.Invoice.PaymentStatus == PaymentStatus.PAID);
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
                d.Invoice.PaymentStatus == PaymentStatus.PAID);
        }

        public async Task<MovieAccessDTO> GetMovieAccessAsync(int movieId)
        {
            var movie = await GetByIdAsync(movieId)
                ?? throw new Exception("Movie not found");
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            // 🎬 FREE
            if (await IsFreeMovieAsync(movieId))
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsFree = true,
                    CanWatch = true
                };
            }

            // 📦 GÓI
            if (await HasActivePackageAsync(userId))
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsFree = false,
                    CanWatch = true
                };
            }

            // 🎥 MUA / THUÊ
            if (await HasMovieAccessAsync(userId, movieId))
            {
                return new MovieAccessDTO
                {
                    MovieId = movieId,
                    IsFree = false,
                    CanWatch = true
                };
            }

            // ❌ KHÔNG CÓ QUYỀN → SHOW GIÁ
            var pricing = await GetActiveAsync(movieId);

            return new MovieAccessDTO
            {
                MovieId = movieId,
                IsFree = false,
                CanWatch = false,
                ShowBuy = pricing.Any(x => x.PricingType == PricingType.BUY),
                ShowRent = pricing.Any(x => x.PricingType == PricingType.RENT),
                BuyPrice = pricing.FirstOrDefault(x => x.PricingType == PricingType.BUY)?.Price,
                RentPrice = pricing.FirstOrDefault(x => x.PricingType == PricingType.RENT)?.Price,
                RentalDurationDays = pricing.FirstOrDefault(x => x.PricingType == PricingType.RENT)?.RentalDurationDays
            };
        }
    }
}