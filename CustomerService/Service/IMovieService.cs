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
        Task<PagedResult<MovieDTO>> SearchMoviesAsync(MovieQueryDTO query);
    }

    public class MovieService : IMovieService
    {

        private readonly dbMoviesContext _context;
        public IMovieRepository _moviesRepository;
        public IUnitOfWork _dbu;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;

        public MovieService(dbMoviesContext context, IMovieRepository moviesRepository, IUnitOfWork dbu,  IHttpContextAccessor httpContextAccessor, IFileService fileService)
        {
            _context = context;
            _moviesRepository = moviesRepository;
            _dbu = dbu;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;  // <-- FIXED
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


    }
}