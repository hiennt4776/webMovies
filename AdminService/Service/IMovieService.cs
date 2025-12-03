using AdminService.Services;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace AdminService.Service
{
    public interface IMovieService
    {
        Task<Movie> CreateMovieAsync(MovieInContractDTO dto);
        Task<PagedResult<MovieDTO>> GetMoviesAsync(string? search, int page, int pageSize);
        Task<MovieDTO?> GetMovieByIdAsync(int id);

        Task AddMovieAsync(MovieDTO movieDTO);
        Task UpdateMovieAsync(MovieDTO dto);

        Task DeleteMovieAsync(int id);

        Task<Movie?> GetMovieContainingFileAsync(int fileId);

        Task<Movie?> GetByIdAsync(int id);
        Task AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);


        Task<MovieDTO> CreateMovieAsync(MovieDTO dto); // optional
    }
    public class MovieService : IMovieService
    {
        private readonly dbMoviesContext _context;
        private readonly IMovieRepository _movieRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;



        public MovieService(dbMoviesContext context, IMovieRepository movieRepository,
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

        public async Task<Movie> CreateMovieAsync(MovieInContractDTO dto)
        {

            var httpContext = _httpContextAccessor.HttpContext
             ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);


            var movie = new Movie
            {
                Title = dto.Title,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                IsDeleted = false,
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<PagedResult<MovieDTO>> GetMoviesAsync(string? search, int page, int pageSize)
        {
            var q = _context.Movies
                .Include(m => m.MovieFiles)
                .Where(m => m.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(m => m.Title.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(m => m.CreatedDate)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            var dtos = items.Select(m => Map(m)).ToList();

            return new PagedResult<MovieDTO>
            {
                Items = dtos,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };

      
        }

      
        public async Task<MovieDTO?> GetMovieByIdAsync(int id)
        {
            var m = await _context.Movies
                .Include(m => m.MovieFiles)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted == false);
            if (m == null) return null;
            return Map(m);
        }

 
        public async Task AddMovieAsync(MovieDTO movieDTO)
        {
            var m = new Movie
            {
                Title = movieDTO.Title,
                Director = movieDTO.Director,
                ReleaseDate = movieDTO.ReleaseDate,
                LanguageList = movieDTO.LanguageList,
                Country = movieDTO.Country,
                Status = movieDTO.Status,
                Budget = movieDTO.Budget,
                BoxOffice = movieDTO.BoxOffice,
                CreatedDate = System.DateTime.Now
            };
            _context.Movies.Add(m);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateMovieAsync(MovieDTO dto)
        {

            var movie = await _context.Movies
               .Include(m => m.MovieFiles)
               .FirstOrDefaultAsync(m => m.Id == dto.Id && m.IsDeleted == false);
            if (movie == null) return;
            movie.Title = dto.Title;
            movie.Director = dto.Director;
            movie.ReleaseDate = dto.ReleaseDate;
            movie.LanguageList = dto.LanguageList;
            movie.Country = dto.Country;
            movie.Status = dto.Status;
            movie.Budget = dto.Budget;
            movie.BoxOffice = dto.BoxOffice;
            movie.UpdatedDate = System.DateTime.Now;
     
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                movie.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Movie?> GetMovieContainingFileAsync(int fileId)
        {
            return await _context.Movies
                .Include(m => m.MovieFiles)
                .Where(m => m.IsDeleted == false && m.MovieFiles.Any(f => f.Id == fileId))
                .FirstOrDefaultAsync();
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


        ////////////
        ///

        //public async Task<PagedResult<Movie>> GetMoviesAsync(string? search, int page, int pageSize)
        //{
        //    var query = _context.Movies
        //        .Where(m => m.IsDeleted == false);

        //    if (!string.IsNullOrWhiteSpace(search))
        //        query = query.Where(m => m.Title.Contains(search));

        //    var total = await query.CountAsync();

        //    var items = await query
        //        .OrderByDescending(m => m.CreatedDate)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Include(m => m.MovieFiles.Where(f => f.IsDeleted == false))
        //        .ToListAsync();

        //    return new PagedResult<Movie>
        //    {
        //        Items = items,
        //        TotalCount = total,
        //        PageNumber = page,
        //        PageSize = pageSize
        //    };
        //}

        public async Task<Movie?> GetByIdAsync(int id)
            => await _context.Movies.Include(m => m.MovieFiles.Where(f => f.IsDeleted == false))
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted == false);

        public async Task AddAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }


        public Task<MovieDTO> CreateMovieAsync(MovieDTO dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                Director = dto.Director,
                Cast = dto.Cast,
                ReleaseDate = dto.ReleaseDate,
                LanguageList = dto.LanguageList,
                Country = dto.Country,
                AgeLimit = dto.AgeLimit,
                Rating = dto.Rating,
                Budget = dto.Budget,
                BoxOffice = dto.BoxOffice,
                Status = dto.Status,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            return Task.Run(async () =>
            {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
                return dto;
            });
        }
    }
}
