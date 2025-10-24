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

    }

    public class MovieService : IMovieService
    {

        private readonly dbMoviesContext _context;
        public IMovieRepository _moviesRepository;
        public IUnitOfWork _dbu;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieService(dbMoviesContext context, IMovieRepository moviesRepository, IUnitOfWork dbu,  IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _moviesRepository = moviesRepository;
            _dbu = dbu;
            _httpContextAccessor = httpContextAccessor;
        }

        //public async Task<List<MovieDTO>> GetRandomTop5FilmsAsync(string baseUrl)
        //{


        //var k = await _context.Movies
        //    .Where(m => m.IsDeleted == false
        //        && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == "POSTER"))
        //    .OrderBy(m => Guid.NewGuid())
        //    .Take(5)
        //    .Select(m => new MovieDTO
        //    {
        //        Id = m.Id,
        //        Title = m.Title,
        //        ReleaseDate = m.ReleaseDate,
        //        FilePath = baseUrl + "/movies/" +
        //            m.MovieFiles
        //                .Where(f => f.IsDeleted == false && f.FileType == "POSTER")
        //                .Select(f => f.FilePath.Replace("\\", "/"))
        //                .FirstOrDefault()



        //    })
        //    .ToListAsync();

        //return await _context.Movies
        //    .Where(m => m.IsDeleted == false
        //        && m.MovieFiles.Any(f => f.IsDeleted == false && f.FileType == "POSTER"))
        //    .OrderBy(m => Guid.NewGuid())
        //    .Take(5)
        //    .Select(m => new MovieDTO
        //    {
        //        Id = m.Id,
        //        Title = m.Title,
        //        ReleaseDate = m.ReleaseDate,
        //        FilePath = baseUrl + "/movies/" +
        //            m.MovieFiles
        //                .Where(f => f.IsDeleted == false && f.FileType == "POSTER")
        //                .Select(f => f.FilePath.Replace("\\", "/"))
        //                .FirstOrDefault()


        //    })
        //    .ToListAsync();

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

    }
}