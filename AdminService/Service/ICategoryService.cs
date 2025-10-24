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
    public interface ICategoryService
    {
        public Task<IEnumerable<CategoryDTO>> GetAllAsync();
        public Task<PagedResult<CategoryDTO>> GetPagedAsync(int pageNumber, int pageSize);
        public Task<CategoryDTO> GetByIdAsync(int id);
        public Task<PagedResult<CategoryDTO>> GetPagedSortSearchAsync(int pageNumber, int pageSize, string? search, string? sortField, bool ascending);
        public Task<CategoryDTO> AddAsync(CategoryDTO dto);
        public Task<CategoryDTO> UpdateAsync(int id, CategoryDTO dto);
        public Task<bool> DeleteAsync(int id);

    }
    public class CategoryService : ICategoryService
    {
        private readonly dbMoviesContext _context;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CategoryService(dbMoviesContext context,
            ICategoryRepository categoryRepository,
                        
            IAuthService authService,
        IUnitOfWork dbu, 
        JwtAuthService jwtAuthService, 
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _categoryRepository = categoryRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            return await _context.Categories
               .Where(c => c.IsDeleted == false)
               .OrderByDescending(c => c.CreatedDate)
               .Select(c => new CategoryDTO
               {
                   Id = c.Id,
                   CategoryName = c.CategoryName,
                   Description = c.Description,
                   Color = c.Color,

               })
               .ToListAsync();
        }


        
        public async Task<PagedResult<CategoryDTO>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var baseQuery = _context.Categories
                .Where(c => c.IsDeleted == false);

            var totalCount = await baseQuery.CountAsync();

            var data = await baseQuery
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    Color = c.Color
                })
                .ToListAsync();

            return new PagedResult<CategoryDTO>
            {
                Items = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<PagedResult<CategoryDTO>> GetPagedSortSearchAsync(int pageNumber, int pageSize, string? search, string? sortField, bool ascending)
        {

            var query = _context.Categories.Where(c => c.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.CategoryName.Contains(search));

            // Sort động
            query = sortField switch
            {
                "CategoryName" => ascending ? query.OrderBy(c => c.CategoryName) : query.OrderByDescending(c => c.CategoryName),
                "CreateDate" => ascending ? query.OrderBy(c => c.CreatedDate) : query.OrderByDescending(c => c.CreatedDate),
                _ => query.OrderByDescending(c => c.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    Color = c.Color
                
                })
                .ToListAsync();

            return new PagedResult<CategoryDTO>
            {
                Items = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDTO
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                Color = category.Color
            };
        }

        public async Task<CategoryDTO> AddAsync(CategoryDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var category = new Category
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                Color = dto.Color,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                IsDeleted = false,
            };

            await _categoryRepository.AddAsync(category);
            await _dbu.SaveChangesAsync();
            dto.Id = category.Id;
            return dto;
        }

        public async Task<CategoryDTO> UpdateAsync(int id, CategoryDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
             ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            category.CategoryName = dto.CategoryName;
            category.Description = dto.Description;
            category.Color = dto.Color;
            category.UpdatedBy = userId;
            category.UpdatedDate = DateTime.Now;

            await _categoryRepository.Update(category);
            await _dbu.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var httpContext = _httpContextAccessor.HttpContext
             ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.IsDeleted = true;
            category.UpdatedBy = userId;
            category.UpdatedDate = DateTime.Now;

            await _categoryRepository.Update(category);
            await _dbu.SaveChangesAsync();
            return true;
        }


    }
}