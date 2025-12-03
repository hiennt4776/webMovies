
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
    public interface ICustomerService
    {

        Task<CustomerDTO> GetProfileAsync(string username);

    }
    public class CustomerDetailService : ICustomerService
    {
        private readonly dbMoviesContext _context;
        public readonly IUserCustomerRepository _userCustomerRepository;
        public readonly ICustomerRepository _CustomerRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerDetailService(dbMoviesContext context,
            IUserCustomerRepository userCustomerRepository,
            ICustomerRepository CustomerRepository,
            IUnitOfWork dbu
            , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userCustomerRepository = userCustomerRepository;
            _CustomerRepository = CustomerRepository;
            _dbu = dbu;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<CustomerDTO> GetProfileAsync(string username)
        {
            var user = await _context.UserCustomers
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.Customer == null) return null;

            // Map entity -> DTO
            return new CustomerDTO //nó bị lỗi ở admin hay user vậy bạn 
            //moi lan login se bao
            //Path: /api/UserCustomer/profile
            //Authorization: No token
            {
                FullName = user.Customer.FullName,
                Email = user.Customer.Email,
                Phone = user.Customer.Phone,
                Address = user.Customer.Address,
                DateOfBirth = user.Customer.DateOfBirth
            };


        }
     

    }
}
