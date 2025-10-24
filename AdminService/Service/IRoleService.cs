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
    public interface IRoleService
    {
        public Task<IEnumerable<RoleDTO>> GetAllAsync();

    }
    public class RoleService : IRoleService
    {
        private readonly dbMoviesContext _context;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public RoleService(dbMoviesContext context, IRoleRepository roleRepository,
                        IAuthService authService,
        IUnitOfWork dbu, JwtAuthService jwtAuthService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _roleRepository = roleRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<RoleDTO>> GetAllAsync()
        {
            return await _context.Roles
               .Select(c => new RoleDTO
               {
                   Id = c.Id,
                   RoleName = c.RoleName

               })
               .ToListAsync();
        }


    }
}