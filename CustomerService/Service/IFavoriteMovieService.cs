
using Confluent.Kafka;
using dbMovies.Models;
using helperMovies.Constants;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace CustomerService.Service
{
    public interface IFavoriteMovieService
    {
        Task<bool> ToggleFavoriteAsync(int movieId);
        Task<bool> IsFavoriteAsync(int movieId);
    }

    public class FavoriteMovieService : IFavoriteMovieService
    {
        private readonly dbMoviesContext _context;
        private readonly IFavoriteMovieRepository _favoriteMovieRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FavoriteMovieService(
            dbMoviesContext context,
               IFavoriteMovieRepository favoriteMovieRepository,
              IAuthService authService,
            IUnitOfWork dbu,
            IConfiguration config,
            JwtAuthService jwtAuthService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _favoriteMovieRepository = favoriteMovieRepository;
            _authService = authService;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> ToggleFavoriteAsync(int movieId)
        {

            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            var favorite = await _context.FavoriteMovies
                .FirstOrDefaultAsync(x => x.UserCustomerId == userId && x.MovieId == movieId);

            if (favorite != null)
            {
                _context.FavoriteMovies.Remove(favorite);
                await _context.SaveChangesAsync();
                return false; // ❌ Đã bỏ yêu thích
            }

            _context.FavoriteMovies.Add(new FavoriteMovie
            {
                UserCustomerId = userId,
                MovieId = movieId,
                AddedDate = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
            return true; // ❤️ Đã thêm yêu thích
        }

        public async Task<bool> IsFavoriteAsync(int movieId)
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var k = await _context.FavoriteMovies
                .AnyAsync(x => x.UserCustomerId == userId && x.MovieId == movieId);

            return await _context.FavoriteMovies
                .AnyAsync(x => x.UserCustomerId == userId && x.MovieId == movieId);
        }

    }
        
}


