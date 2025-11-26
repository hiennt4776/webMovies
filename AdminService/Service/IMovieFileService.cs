using AdminService.Services;
using AdminService.Utils;
using Confluent.Kafka;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace AdminService.Service
{
    public interface IMovieFileService
    {

        Task<List<MovieFileDTO>> GetFilesAsync(int movieId);
        Task<MovieFileDTO> UploadFileAsync(int movieId, IFormFile file, string fileType);
        Task DeleteFileAsync(int fileId);
        Task<(byte[] content, string contentType, string fileName)?> DownloadFileAsync(int fileId);
        string GetContentType(string path);
        Task<PagedResult<Movie>> GetMoviesAsync(string? search, int page, int pageSize);
        Task<Movie?> GetMovieByIdAsync(int id);
        Task AddMovieAsync(Movie movie);
        Task UpdateMovieAsync(Movie movie);
        Task DeleteMovieAsync(int id);
        Task<Movie> GetMovieContainingFileAsync(int fileId);


        Task<List<MovieFile>> GetFilesByMovieIdAsync(int movieId);
        Task<MovieFile?> GetByIdAsync(int id);
        Task AddAsync(MovieFile file);
        Task UpdateAsync(MovieFile file);



    }
    public class MovieFileService : IMovieFileService
    {
        private readonly dbMoviesContext _context;
        private readonly IMovieRepository _movieRepository;
        private readonly IMovieFileRepository _movieFileRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMovieService _movieService;
        private readonly IFileService _fileService;
        private readonly string _fileBasePath;
        private readonly IConfiguration _config;

        public MovieFileService(dbMoviesContext context, IMovieRepository movieRepository, IMovieFileRepository movieFileRepository,
        IAuthService authService,
        IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor,
        IMovieService movieService, IFileService fileService, IOptions<FileSettings> settings,
         IConfiguration config
            )
        {
            _context = context;
            _movieRepository = movieRepository;
            _movieFileRepository = movieFileRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
            _movieService = movieService;
            _fileService = fileService;
            _fileBasePath = config["FileSettings:FilesPath"]!;
             _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public MovieDTO MapToDto(Movie m)
        {
            return new MovieDTO
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
                Files = m.MovieFiles.Where(f => f.IsDeleted == false).Select(f => new MovieFileDTO
                {
                    Id = f.Id,
                    FileType = f.FileType,
                    FileName = f.FileName,
                    FilePath = f.FilePath
                }).ToList()
            };
        }

        public async Task<MovieFileDTO> UploadFileAsync(int movieId, IFormFile file, string fileType)
        {

            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            // ensure movie exists
            var movie = await GetMovieByIdAsync(movieId);
            if (movie == null) throw new KeyNotFoundException("Movie not found");
            var _baseFilesPath = _config["FileSettings:FilesPath"] ?? throw new InvalidOperationException("FilesPath not configured");
            var movieFolder = Path.Combine(_baseFilesPath, "movies", $"M{movieId}");
            if (!Directory.Exists(movieFolder))
                Directory.CreateDirectory(movieFolder);

            var fileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            // Optional: sanitize fileName or prefix with GUID
            var uniqueFileName = $"{Guid.NewGuid():N}_{extension}";
            var filePath = Path.Combine(movieFolder, uniqueFileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

        

            var fileEntity = new MovieFile
            {
                MovieId = movieId,
                FileType = fileType,
                FileName = fileName,
                FilePath = Path.Combine("movies", $"M{movieId}", uniqueFileName),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            };

            await AddAsync(fileEntity);

            return new MovieFileDTO
            {
                Id = fileEntity.Id,
                MovieId = fileEntity.MovieId,
                FileType = fileEntity.FileType,
                FileName = fileEntity.FileName,
                FilePath = fileEntity.FilePath
            };
        }

        public async Task DeleteFileAsync(int fileId)
        {
            var movie = await GetMovieContainingFileAsync(fileId);
            if (movie == null) return;

            var file = movie.MovieFiles.FirstOrDefault(f => f.Id == fileId);
            if (file == null) return;

            if (File.Exists(file.FilePath))
            {
                try { File.Delete(file.FilePath); } catch { /* ignore */ }
            }

            file.IsDeleted = true;
            await UpdateMovieAsync(movie);
        }

        public async Task<(byte[] content, string contentType, string fileName)?> DownloadFileAsync(int fileId)
        {
            var movie = await GetMovieContainingFileAsync(fileId);
            if (movie == null) return null;
            var file = movie.MovieFiles.FirstOrDefault(f => f.Id == fileId && f.IsDeleted == false);
            if (file == null) return null;
            if (!File.Exists(file.FilePath)) return null;

            var bytes = await File.ReadAllBytesAsync(file.FilePath);
            var contentType = GetContentType(file.FilePath);
            return (bytes, contentType, file.FileName);
        }

        public string GetContentType(string path)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType)) contentType = "application/octet-stream";
            return contentType;
        }

        public async Task<PagedResult<Movie>> GetMoviesAsync(string? search, int page, int pageSize)
        {
            var query = _context.Movies.Where(m => m.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => EF.Functions.Like(m.Title, $"%{search}%"));

            var total = await query.CountAsync();

            var items = await query
                .Include(m => m.MovieFiles)
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Movie>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.Include(m => m.MovieFiles).FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted == false);

        }
        public async Task AddMovieAsync(Movie movie)
        {
            await _movieRepository.AddAsync(movie);
            await _dbu.SaveChangesAsync();
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            await _movieRepository.Update(movie);
            await _dbu.SaveChangesAsync();
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
        public async Task<Movie> GetMovieContainingFileAsync(int fileId)
        {
            return await _context.Movies.Include(m => m.MovieFiles)
                .FirstOrDefaultAsync(m => m.MovieFiles.Any(f => f.Id == fileId));
        }

        public async Task<List<MovieFile>> GetFilesByMovieIdAsync(int movieId)
       => await _context.MovieFiles.Where(f => f.MovieId == movieId && f.IsDeleted == false).ToListAsync();

        public async Task<MovieFile?> GetByIdAsync(int id)
            => await _context.MovieFiles.FirstOrDefaultAsync(f => f.Id == id && f.IsDeleted == false);

        public async Task AddAsync(MovieFile file)
        {
            _context.MovieFiles.Add(file);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MovieFile file)
        {
            _context.MovieFiles.Update(file);
            await _context.SaveChangesAsync();
        }


        public async Task<List<MovieFileDTO>> GetFilesAsync(int movieId)
        {
            var files = await GetFilesByMovieIdAsync(movieId);
            return files.Select(f => new MovieFileDTO
            {
                Id = f.Id,
                MovieId = f.MovieId,
                FileType = f.FileType,
                FileName = f.FileName,
                FilePath = f.FilePath,
            }).ToList();
        }

     
    }
}
