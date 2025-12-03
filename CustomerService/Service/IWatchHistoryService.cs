
using dbMovies.Models;
using helperMovies.Constants;
using helperMovies.DTO;
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
    public interface IWatchHistoryService
    {
        Task<decimal> GetWatchTimeAsync(int movieId);
        Task SaveWatchTimeAsync(WatchHistoryDTO req);
    }
    public class WatchHistoryService : IWatchHistoryService
    {
        private readonly dbMoviesContext _context;

        private readonly IWatchHistoryRepository _watchHistoryRepository;
        private readonly IUserCustomerRepository _userCustomerRepository; 
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public async Task<decimal> GetWatchTimeAsync(int movieId)
        {
            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            var item = await _context.WatchHistories
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId);

            return item?.CurrentTime ?? 0;
        }

        public async Task SaveWatchTimeAsync(WatchHistoryDTO req)
        {
            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var item = await _context.WatchHistories
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == req.MovieId);

            if (item == null)
            {
                item = new WatchHistory
                {
                    UserId = userId,
                    MovieId = req.MovieId,
                    CurrentTime = req.CurrentTime,
                    UpdateAt = DateTime.Now
                };
                _context.WatchHistories.Add(item);
            }
            else
            {
                item.CurrentTime = req.CurrentTime;
                item.UpdateAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }

}
