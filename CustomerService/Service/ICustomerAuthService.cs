//using CustomerService.Utils;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Identity;
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

namespace CustomerService.Service
{
    public interface ICustomerAuthService
    {
        int GetUserIdFromToken(HttpContext httpContext);
        string? GetUserNameFromToken(HttpContext httpContext);
    }
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly dbMoviesContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserCustomerRepository _userCustomerRepository;


        public CustomerAuthService(
            ICustomerRepository customerRepository,
            IUserCustomerRepository userCustomerRepository)
        {
            _customerRepository = customerRepository;
            _userCustomerRepository = userCustomerRepository;

        }

        public int GetUserIdFromToken(HttpContext httpContext)
        {
            var claim = httpContext.User?.FindFirst("UserId");
            if (claim == null) throw new UnauthorizedAccessException("Token không có UserId");
            if (!int.TryParse(claim.Value, out int userId))
                throw new InvalidOperationException("UserId không hợp lệ trong token");
            return userId;
        }

        public string? GetUserNameFromToken(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst("UserName")?.Value;
        }

        
    }

}
