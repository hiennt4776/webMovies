using AdminService.Services;
using dbMovies.Models;
using helperMovies.DTO;
using System;

namespace AdminService.Service
{
    public interface IMovieService
    {
        Task<Movie> CreateMovieAsync(MovieInContractDTO dto);
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
    }
}
