//using CustomerService.Utils;
using dbMovies.Models;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace CustomerService.Service
{

    
    public interface IMovieService
    {
        Task<List<MovieDTO>> GetRandomTop5FilmsAsync();  // đổi trả DTO
        Task<List<MovieDTO>> GetMoviesAsync();
        Task<MovieDTO> GetMovieByIdAsync(int movieId);
        Task<byte[]> GetMovieFileAsync(int movieId, string fileType);
    }

    public class MovieService : IMovieService
    {

        private readonly dbMoviesContext _context;
        public IMovieRepository _moviesRepository;
        public IUnitOfWork _dbu;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FileService _fileService;

        public MovieService(dbMoviesContext context, IMovieRepository moviesRepository, IUnitOfWork dbu,  IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _moviesRepository = moviesRepository;
            _dbu = dbu;
            _httpContextAccessor = httpContextAccessor;
        }

      

        public async Task<List<MovieDTO>> GetRandomTop5FilmsAsync()
        {

            var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            var k = await _context.Movies
                  .Where(m => m.IsDeleted == false
                      && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == "POSTER"))
                  .OrderBy(m => Guid.NewGuid())
                  .Take(5)
                  .Select(m => new MovieDTO
                  {
                      Id = m.Id,
                      Title = m.Title,
                      ReleaseDate = m.ReleaseDate,
                      // chỉ thêm /movies nếu đường dẫn trong DB KHÔNG chứa nó
                      FilePath = $"{baseUrl}/{m.MovieFiles
                          .Where(f => f.IsDeleted == false && f.FileType == "POSTER")
                          .Select(f => f.FilePath.Replace("\\", "/"))
                          .FirstOrDefault()}"
                  })
                  .ToListAsync();

            return await _context.Movies
                  .Where(m => m.IsDeleted == false
                      && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == "POSTER"))
                  .OrderBy(m => Guid.NewGuid())
                  .Take(5)
                  .Select(m => new MovieDTO
                  {
                      Id = m.Id,
                      Title = m.Title,
                      ReleaseDate = m.ReleaseDate,
                      // chỉ thêm /movies nếu đường dẫn trong DB KHÔNG chứa nó
                      FilePath = $"{baseUrl}/{m.MovieFiles
                          .Where(f => f.IsDeleted == false && f.FileType == "POSTER")
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
                    HasTrailer = m.MovieFiles.Any(f => f.FileType.ToLower() == "trailer" && f.IsDeleted == false),
                    HasMovie = m.MovieFiles.Any(f => f.FileType.ToLower() == "movies" && f.IsDeleted == false),
                    HasSubtitle = m.MovieFiles.Any(f => f.FileType.ToLower() == "subtitle" && f.IsDeleted == false)
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
                    HasTrailer = m.MovieFiles.Any(f => f.FileType.ToLower() == "trailer" && f.IsDeleted == false),
                    HasMovie = m.MovieFiles.Any(f => f.FileType.ToLower() == "movies" && f.IsDeleted == false),
                    HasSubtitle = m.MovieFiles.Any(f => f.FileType.ToLower() == "subtitle" && f.IsDeleted == false)
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

    }
}