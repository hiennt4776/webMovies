using AdminService.Services;
using dbMovies.Models;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;


namespace AdminService.Service
{
    public interface IUserEmployeeService
    {
        public Task<LoginEmployeeResponseDTO> Login(LoginEmployeeRequestDTO model);
        public Task<UserEmployee?> FindUserByUsernameAsync(string usernameOrEmail);
        Task<PagedResult<UserEmployeeDTO>> GetPagedAsync(string? keyword, int? roleId, bool? isLocked, int page, int pageSize);
        public Task<int> CreateAsync(UserEmployeeCreateDTO dto);

        public Task<UserEmployeeUpdateDTO> UpdateAsync(UserEmployeeUpdateDTO dto);

        public Task<UserEmployeeResetPasswordDTO> ResetPasswordAsync(UserEmployeeResetPasswordDTO dto);
        public Task<int> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);
        public Task<UserEmployeeDTO> GetUserEmployeesAsync(int userid);
        public Task<UserEmployeeUpdateDTO> GetUserEditEmployeesAsync(int userid);

    }
    public class UserEmployeeService : IUserEmployeeService
    {
        private readonly dbMoviesContext _context;
        public readonly IUserEmployeeRepository _userEmployeeRepository;
        public readonly IEmployeeRepository _employeeRepository;
        public readonly IRoleUserRepository _roleUserRepository;

        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEmployeeService(dbMoviesContext context,
                    IUserEmployeeRepository userEmployeeRepository,
            IEmployeeRepository employeeRepository,
                        IRoleUserRepository roleUserRepository,
            IRoleRepository roleRepository,

           IAuthService authService,
        IUnitOfWork dbu,
        JwtAuthService jwtAuthService,
        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userEmployeeRepository = userEmployeeRepository;
            _roleUserRepository = roleUserRepository;
            _employeeRepository = employeeRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginEmployeeResponseDTO> Login(LoginEmployeeRequestDTO loginUserEmployeeDTO)
        {
            try
            {

                var user = await _context.UserEmployees
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == loginUserEmployeeDTO.Username);

                if (user == null || !PasswordHelper.VerifyPassword(loginUserEmployeeDTO.Password, user.PasswordHash))
                    return new LoginEmployeeResponseDTO
                    {
                        messenger = MessageLogin.UserNotFound// Người dùng không tồn tại        s
                    };




                //var user = await _userEmployeeRepository.SingleOrDefaultAsync(us => us.Username == loginUserEmployeeDTO.Username);
                //if (user == null)
                //{
                //    return null;  
                //}

                //Nếu user tồn tại thì kiểm tra mật khẩu
                if (PasswordHelper.VerifyPassword(loginUserEmployeeDTO.Password, user.PasswordHash))
                {
                    //Trả về access token 
                    //LoginEmployeeResponseDTO
                    //return _jwtAuthService.GenerateToken(user);

                    return new LoginEmployeeResponseDTO
                    {
                        Token = _jwtAuthService.GenerateToken(user),
                        FullName = user.Employee.FullName ?? "",
                        Email = user.Employee.Email ?? ""
                    };
                }


                return new LoginEmployeeResponseDTO
                {
                    messenger = MessageLogin.PasswordIncorrect// Người dùng không tồn tại        s
                };

            }
            catch (Exception ex)
            {
                return null;
            }
        }

   
        public async Task<UserEmployee?> FindUserByUsernameAsync(string usernameOrEmail)
        {
            return await _context.UserEmployees
                .Include(u => u.RoleUsers)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                .Where(u => u.Username == usernameOrEmail)
                .SingleOrDefaultAsync();
        }

     public async Task<PagedResult<UserEmployeeDTO>> GetPagedAsync(string? keyword, int? roleId, bool? isLocked, int page, int pageSize)
        {
            var query = _context.UserEmployees
                .Include(u => u.Employee)
                .Include(u => u.RoleUsers)
                    .ThenInclude(ru => ru.Role)
                .AsQueryable();

            // 🔍 Search theo username hoặc tên nhân viên
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => 
                    u.Username.Contains(keyword) || 
                    (u.Employee != null && u.Employee.FullName.Contains(keyword)));
            }

            // 🧩 Filter theo Role
            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleUsers.Any(ru => ru.RoleId == roleId.Value));
            }

            // 🔒 Filter theo trạng thái khóa
            if (isLocked.HasValue)
            {
                query = query.Where(u => u.IsLocked == isLocked.Value);
            }

            // Tổng số bản ghi
            var totalCount = await query.CountAsync();

            // Phân trang
            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserEmployeeDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Employee = u.Employee == null ? null : new EmployeeDTO
                    {
                        Id = u.Employee.Id,
                        FullName = u.Employee.FullName
                    },
                    Role = u.RoleUsers.Select(ru => new RoleDTO
                    {
                        Id = ru.Role.Id,
                        RoleName = ru.Role.RoleName
                    }).FirstOrDefault(),
                    IsLocked = u.IsLocked,
                    ReasonLock = u.ReasonLock
                })
                .ToListAsync();

            return new PagedResult<UserEmployeeDTO>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateAsync(UserEmployeeCreateDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            // kiểm tra username trùng
            if (await _context.UserEmployees.AnyAsync(u => u.Username == dto.Username))
                throw new Exception("Tên đăng nhập đã tồn tại.");

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                throw new Exception("Nhân viên không tồn tại.");

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                throw new Exception("Vai trò không tồn tại.");

            // mã hóa mật khẩu đơn giản
            await _dbu.BeginTransactionAsync();

            var userEmployee = new UserEmployee
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmployeeId = dto.EmployeeId,
                IsLocked = dto.IsLocked,
                ReasonLock = dto.ReasonLock ?? "",
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };
            await _userEmployeeRepository.AddAsync(userEmployee);

            // thêm vào bảng RoleUsers

        
            var roleUser = new RoleUser
            {
                User = userEmployee,
                RoleId = dto.RoleId
            };
            await _roleUserRepository.AddAsync(roleUser);
            await _dbu.SaveChangesAsync();

            await _dbu.CommitTransactionAsync();

            return userEmployee.Id;
        }

        public async Task<UserEmployeeUpdateDTO> UpdateAsync(UserEmployeeUpdateDTO userEmployeeUpdateDTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var user = await _context.UserEmployees
                .Include(u => u.RoleUsers)
                .FirstOrDefaultAsync(u => u.Id == userEmployeeUpdateDTO.Id);

            //if (!string.IsNullOrWhiteSpace(userEmployeeUpdateDTO.Password))
            //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userEmployeeUpdateDTO.Password);

            user.IsLocked = userEmployeeUpdateDTO.IsLocked;
            user.ReasonLock = userEmployeeUpdateDTO.ReasonLock;
            user.UpdatedBy = userId;
            user.UpdatedDate = DateTime.Now;

            var existingRoleUser = user.RoleUsers.FirstOrDefault();
            if (existingRoleUser != null)
            {
                existingRoleUser.RoleId = userEmployeeUpdateDTO.RoleId;
            }
            else
            {
                _context.RoleUsers.Add(new RoleUser { RoleId = userEmployeeUpdateDTO.RoleId, UserId = user.Id });
            }

            await _dbu.SaveChangesAsync();
            return userEmployeeUpdateDTO;
        }

        public async Task<UserEmployeeResetPasswordDTO> ResetPasswordAsync(UserEmployeeResetPasswordDTO userEmployeeResetPasswordDTO)
        {
            //var httpContext = _httpContextAccessor.HttpContext
            //     ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            //int userId = _authService.GetUserIdFromToken(httpContext);


            //var user = await _context.UserEmployees
            //    .Include(u => u.RoleUsers)
            //    .FirstOrDefaultAsync(u => u.Id == userEmployeeResetPasswordDTO.Id);

            //if (!string.IsNullOrWhiteSpace(userEmployeeResetPasswordDTO.Password))
            //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userEmployeeResetPasswordDTO.Password);


            //await _dbu.SaveChangesAsync();
            //return userEmployeeResetPasswordDTO;

            var httpContext = _httpContextAccessor.HttpContext
      ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            //var user = await _context.UserEmployees.FindAsync(model.Id);
            //if (user == null) return false;
            //if (string.IsNullOrWhiteSpace(model.Password)) return false;

            var user = await _context.UserEmployees
                .Include(u => u.RoleUsers)
                .FirstOrDefaultAsync(u => u.Id == userEmployeeResetPasswordDTO.Id);

            if (!string.IsNullOrWhiteSpace(userEmployeeResetPasswordDTO.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userEmployeeResetPasswordDTO.Password);


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userEmployeeResetPasswordDTO.Password);
            user.UpdatedBy = userId;
            user.UpdatedDate = DateTime.Now;
            await _dbu.SaveChangesAsync();
            return userEmployeeResetPasswordDTO;
        }

        public async Task<int> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);



            var user = await _userEmployeeRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng.");
            if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");

            if (PasswordHelper.VerifyPassword(changePasswordDTO.OldPassword, user.PasswordHash))
            {
                throw new Exception("Mật khẩu xác nhận không khớp.");
            }

            //return MessageLogin.PasswordIncorrect;

            var verifyResult = PasswordHelper.VerifyPassword(changePasswordDTO.OldPassword, user.PasswordHash);
            if (PasswordHelper.VerifyPassword(changePasswordDTO.OldPassword, user.PasswordHash))
            {

                await _dbu.BeginTransactionAsync();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);

                await _userEmployeeRepository.Update(user);
                await _dbu.SaveChangesAsync();
                await _dbu.CommitTransactionAsync();


                return (user.Id);
            }
            else
            {
                throw new Exception("Mật khẩu xác nhận không khớp.");
            }


        }

        public async Task<UserEmployeeDTO> GetUserEmployeesAsync(int userid)
        {
            var userEmployee = await _userEmployeeRepository.GetByIdAsync(userid);
            if (userEmployee == null) return null;

            return new UserEmployeeDTO
            {
                Id = userEmployee.Id,
                Username = userEmployee.Username,
                Employee = userEmployee.Employee == null ? null : new EmployeeDTO
                {
                    Id = userEmployee.Employee.Id,
                    FullName = userEmployee.Employee.FullName
                },
                Role = userEmployee.RoleUsers.Select(ru => new RoleDTO
                {
                    Id = ru.Role.Id,
                    RoleName = ru.Role.RoleName
                }).FirstOrDefault(),
                IsLocked = userEmployee.IsLocked,
                ReasonLock = userEmployee.ReasonLock
            };
        
        }
        public async Task<UserEmployeeUpdateDTO?> GetUserEditEmployeesAsync(int id)
        {
            var userEmployee = await _context.UserEmployees
            .Include(u => u.Employee)
            .Include(u => u.RoleUsers)
                .ThenInclude(ru => ru.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

            if (userEmployee == null) return null;
            var roleUser = userEmployee.RoleUsers.FirstOrDefault();

            return new UserEmployeeUpdateDTO
            {
                Id = userEmployee.Id,
                Username = userEmployee.Username,
              
                RoleId = (int)(roleUser?.RoleId),
                
                IsLocked = userEmployee.IsLocked,
                ReasonLock = userEmployee.ReasonLock
            };
        }

        public async Task<UserEmployeeResetPasswordDTO> GetResetPasswordInfoAsync(int userId)
        {
            var user = await _context.UserEmployees
                .Include(u => u.Employee)
                .Include(u => u.RoleUsers)
                    .ThenInclude(ru => ru.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var role = user.RoleUsers.FirstOrDefault()?.Role;

            return new UserEmployeeResetPasswordDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.Employee?.FullName,
                RoleName = role?.RoleName,
                IsLocked = user.IsLocked
            };
        }

    }
}

