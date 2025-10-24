using Confluent.Kafka;
using CustomerService.Service;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace CustomerService.Service
{


    public interface IUserCustomerService
    {
        Task<bool> RegisterAsync(CustomerRegisterDTO dto);
        Task<LoginCustomerResponseDTO> LoginAsync(LoginCustomerRequestDTO req);
    }
    public class UserCustomerService : IUserCustomerService
    {
        private readonly dbMoviesContext _context;
        private readonly ICustomerRepository _customersRepository;
        private readonly IUserCustomerRepository _userCustomersRepository;
        private readonly IUnitOfWork _dbu;
        private readonly IConfiguration _config;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserCustomerService(
            dbMoviesContext context,
            ICustomerRepository customersRepository,
            IUserCustomerRepository userCustomersRepository,
              IUnitOfWork dbu,
        IConfiguration config,


         JwtAuthService jwtAuthService,
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _customersRepository = customersRepository;
            _userCustomersRepository = userCustomersRepository;
            _dbu = dbu;
            _config = config;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.UserCustomers.AnyAsync(u => u.Username == username);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> RegisterAsync(CustomerRegisterDTO dto)
        {
            // Kiểm tra trùng username hoặc email
            if (await UsernameExistsAsync(dto.Username))
                throw new Exception("Tên đăng nhập đã tồn tại!");
            if (await EmailExistsAsync(dto.Email))
                throw new Exception("Email đã được sử dụng!");
            await _dbu.BeginTransactionAsync();

            // Tạo Customer
            var customer = new Customer
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                CreatedDate = DateTime.Now
            };
            await _customersRepository.AddAsync(customer);

            // Tạo UserCustomer
            var userCustomer = new UserCustomer
            {
                Username = dto.Username,
                Customer = customer,
                CreatedDate = DateTime.Now,
                IsLocked = false,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),

            };


            await _userCustomersRepository.AddAsync(userCustomer);
            await _dbu.SaveChangesAsync();

            await _dbu.CommitTransactionAsync();

            return true;
        }


        public async Task<LoginCustomerResponseDTO> LoginAsync(LoginCustomerRequestDTO req)
        {
            try
            {
                var user = await _context.UserCustomers
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == req.Username);

                if (user == null)
                return null;



                //Nếu user tồn tại thì kiểm tra mật req
                if (PasswordHelper.VerifyPassword(req.Password, user.PasswordHash))
                {

                    return new LoginCustomerResponseDTO
                    {
                        Token = _jwtAuthService.GenerateToken(user),
                        FullName = user.Customer.FullName ?? "",
                        Email = user.Customer.Email ?? ""
                    };
                }
             return null; 



            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}