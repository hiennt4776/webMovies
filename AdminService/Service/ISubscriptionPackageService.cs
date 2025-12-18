using AdminService.Services;
using dbMovies.Models;
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


namespace AdminService.Service
{
    public interface ISubscriptionPackageService
    {
        Task<PagedResult<SubscriptionPackageDTO>> GetAllPagedAsync(string? search, int page, int pageSize);
        Task<SubscriptionPackageDTO?> GetByIdAsync(int id);
        Task<SubscriptionPackageDTO> CreateAsync(SubscriptionPackageDTO dto);
        Task<SubscriptionPackageDTO> UpdateAsync(int id, SubscriptionPackageDTO dto);
        Task<bool> DeleteAsync(int id);
    }

    public class SubscriptionPackageService : ISubscriptionPackageService
    {
        private readonly dbMoviesContext _context;
        private readonly ISubscriptionPackageRepository _subscriptionPackageRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public SubscriptionPackageService(dbMoviesContext context, 
            ISubscriptionPackageRepository subscriptionPackageRepository,
                        IAuthService authService,
        IUnitOfWork dbu, 
        JwtAuthService jwtAuthService, 
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _subscriptionPackageRepository = subscriptionPackageRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<List<SubscriptionPackageDTO>> GetAllAsync()
        {
            return await _context.SubscriptionPackages
                .Where(x => x.IsDeleted == false)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Price = x.Price
                })
                .ToListAsync();
        }

        public async Task<PagedResult<SubscriptionPackageDTO>> GetAllPagedAsync(string? search, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.SubscriptionPackages.AsQueryable()
                .Where(x => x.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Price = x.Price
                })
                .ToListAsync();

            return new PagedResult<SubscriptionPackageDTO>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<SubscriptionPackageDTO?> GetByIdAsync(int id)
        {
            return await _context.SubscriptionPackages
                .Where(x => x.Id == id && x.IsDeleted == false)
                .Select(x => new SubscriptionPackageDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    DurationMonths = x.DurationMonths,
                    Price = x.Price
                })
                .FirstOrDefaultAsync();
        }

        public async Task<SubscriptionPackageDTO> CreateAsync(SubscriptionPackageDTO DTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
 ?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var entity = new SubscriptionPackage
            {
                Name = DTO.Name,
                DurationMonths = DTO.DurationMonths,
                Price = DTO.Price,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                IsDeleted = false
            };
            await _subscriptionPackageRepository.AddAsync(entity);

            await _context.SaveChangesAsync();

            DTO.Id = entity.Id;
            return DTO;
        }

        public async Task<SubscriptionPackageDTO> UpdateAsync(int id, SubscriptionPackageDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);
            var entity = await _subscriptionPackageRepository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("Package not found");

            entity.Name = dto.Name;
            entity.DurationMonths = dto.DurationMonths;
            entity.Price = dto.Price;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = userId;
            entity.IsDeleted = false;



            await _context.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var httpContext = _httpContextAccessor.HttpContext
?? throw new InvalidOperationException("There is no HttpContext in ContractService");
            int userId = _authService.GetUserIdFromToken(httpContext);

            var entity = await _subscriptionPackageRepository.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == false)
                return false;

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }
    }

}