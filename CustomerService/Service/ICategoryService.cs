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

namespace CustomerService.Service
{
    public interface ICategoryService
    {

        public Task<IEnumerable<CategoryDTO>> GetAllAsync();
    }
    public class CategoryService : ICategoryService
    {
        private readonly dbMoviesContext _context;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CategoryService(dbMoviesContext context,
            ICategoryRepository categoryRepository,

        IUnitOfWork dbu, 
        JwtAuthService jwtAuthService, 
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _categoryRepository = categoryRepository;
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

    }
}