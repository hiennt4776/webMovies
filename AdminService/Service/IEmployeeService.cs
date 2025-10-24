using AdminService.Services;
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



namespace AdminService.Service
{
    public interface IEmployeeService
    {

        Task<EmployeeDTO> GetProfileAsync(string username);
        Task<EmployeeCreateDTO> AddEmployeeAsync(EmployeeCreateDTO employeeCreateDTO);
        Task<EmployeeUpdateDTO> EditEmployeeAsync(int employeeId, EmployeeUpdateDTO employeeUpdateDTO);
        Task<EmployeeUpdateByUserDTO> EmployeeUpdateByUserAsync(int employeeId, EmployeeUpdateByUserDTO dto);
        Task<int> DeleteAsync(int employeeId);
        Task<EmployeeDTO> GetEmployeesAsync(int employeeId);
        Task<PagedResult<EmployeeDTO>> GetEmployeesFilterAsync(EmployeeFilterDTO filter);

        Task<IEnumerable<EmployeeDTO>> GetEmployees();
    }
    public class EmployeeService : IEmployeeService
    {
        private readonly dbMoviesContext _context;
        public readonly IUserEmployeeRepository _userEmployeeRepository;
        public readonly IRoleUserRepository _roleUserRepository;
        public readonly IRoleRepository _roleRepository;
        public readonly IEmployeeRepository _employeeRepository;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _dbu;
        private readonly JwtAuthService _jwtAuthService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeeService(dbMoviesContext context, 
            IUserEmployeeRepository userEmployeeRepository,
            IRoleUserRepository roleUserRepository,
            IRoleRepository roleRepository,
            IEmployeeRepository employeeRepository,
            IAuthService authService,
            IUnitOfWork dbu, 
            JwtAuthService jwtAuthService
            , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userEmployeeRepository = userEmployeeRepository;
            _roleUserRepository = roleUserRepository;
            _roleRepository = roleRepository;
            _employeeRepository = employeeRepository;
            _authService = authService;
            _dbu = dbu;
            _jwtAuthService = jwtAuthService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<EmployeeDTO>> GetEmployees()
        {
            return await _context.Employees
               .Where(p => p.IsDeleted == false)
               .OrderByDescending(p => p.CreatedDate)
                  .Select(e => new EmployeeDTO
                  {
                      Id = e.Id,
                      FullName = e.FullName,
                      Gender = e.Gender,
                      Email = e.Email,
                      PhoneNumber = e.PhoneNumber,
                      Address = e.Address,
                      JobStatus = e.JobStatus,
                      DateOfBirth = e.DateOfBirth,
                      Salary = e.Salary
                  })
                .ToListAsync();

        }

        public async Task<EmployeeDTO> GetProfileAsync(string username)
        {
            var user = await _context.UserEmployees
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.Employee == null) return null;

            // Map entity -> DTO
            return new EmployeeDTO //nó bị lỗi ở admin hay user vậy bạn 
            //moi lan login se bao
            //Path: /api/UserEmployee/profile
            //Authorization: No token
            {
                FullName = user.Employee.FullName,
                Email = user.Employee.Email,
                PhoneNumber = user.Employee.PhoneNumber,
                Address = user.Employee.Address,
                JobStatus = user.Employee.JobStatus,
                DateOfBirth = user.Employee.DateOfBirth,
                Salary = user.Employee.Salary
            };

      
        }
        public async Task<PagedResult<EmployeeDTO>> GetEmployeesFilterAsync(EmployeeFilterDTO filter)
        {
            var query = _context.Employees.AsQueryable();

            // 🔹 Search theo tên, email hoặc số điện thoại
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(keyword) ||
                    e.Email.ToLower().Contains(keyword) ||
                    e.PhoneNumber.ToLower().Contains(keyword));
            }

            // 🔹 Filter JobStatus (sử dụng constant)
            if (!string.IsNullOrWhiteSpace(filter.JobStatus))
            {
                if (filter.JobStatus == JobStatusValue.WORKING ||
                    filter.JobStatus == JobStatusValue.RETIRED)
                {
                    query = query.Where(e => e.JobStatus == filter.JobStatus);
                }
            }

            // 🔹 Tổng số dòng
            var totalCount = await query.CountAsync();

            // 🔹 Phân trang
            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Gender = e.Gender,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    Address = e.Address,
                    JobStatus = e.JobStatus,
                    DateOfBirth = e.DateOfBirth,
                    Salary = e.Salary
                })
                .ToListAsync();

            return new PagedResult<EmployeeDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        public async Task<EmployeeCreateDTO> AddEmployeeAsync(EmployeeCreateDTO employeeCreateDTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
       ?? throw new InvalidOperationException("There is no HttpContext in EmployeeService");

            int userId = _authService.GetUserIdFromToken(httpContext);

            //using var transaction = await _context.Database.BeginTransactionAsync();
            await  _dbu.BeginTransactionAsync();
            try
            {
                // Tạo Employee mới
                var employee = new Employee
                {
                    FullName = employeeCreateDTO.FullName,
                    Gender = employeeCreateDTO.Gender,
                    Email = employeeCreateDTO.Email,
                    Address = employeeCreateDTO.Address,
                    PhoneNumber = employeeCreateDTO.PhoneNumber,
                    JobStatus = employeeCreateDTO.JobStatus,
                    DateOfBirth = employeeCreateDTO.DateOfBirth,
                    Salary = employeeCreateDTO.Salary,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _employeeRepository.AddAsync(employee);

                // Tạo User tương ứng với Employee
                var userEmployee = new UserEmployee
                {
                    Username = employeeCreateDTO.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeeCreateDTO.Password),
                    IsLocked = false,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    Employee = employee
                };
                await _userEmployeeRepository.AddAsync(userEmployee);

                // Gán Role cho User bằng ID, tránh attach sai context
                var roleUser = new RoleUser
                {
                    User = userEmployee,
                    RoleId = employeeCreateDTO.RoleId.Value // chỉ gán ID, không attach Role entity
                };
                await _roleUserRepository.AddAsync(roleUser);

                await _dbu.SaveChangesAsync();
                await _dbu.CommitTransactionAsync();

                return employeeCreateDTO;
            }
            catch
            {
                await _dbu.RollBackTransactionAsync();
                throw;
            }
        }
        public async Task<EmployeeUpdateDTO> EditEmployeeAsync(int employeeId, EmployeeUpdateDTO employeeUpdateDTO)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);

            var currentEmployee = await _employeeRepository.GetByIdAsync(employeeId);
            currentEmployee.FullName = employeeUpdateDTO.FullName;
            currentEmployee.Gender = employeeUpdateDTO.Gender;
            currentEmployee.Email = employeeUpdateDTO.Email;
            currentEmployee.Address = employeeUpdateDTO.Address;
            currentEmployee.PhoneNumber = employeeUpdateDTO.PhoneNumber;
            currentEmployee.JobStatus = employeeUpdateDTO.JobStatus;
            currentEmployee.DateOfBirth = employeeUpdateDTO.DateOfBirth;
            currentEmployee.Salary = employeeUpdateDTO.Salary;
            currentEmployee.UpdatedBy = userId;
            currentEmployee.UpdatedDate = DateTime.Now;

            await _employeeRepository.Update(currentEmployee);
            await _dbu.SaveChangesAsync();

            return employeeUpdateDTO;
        }

        public async Task<EmployeeUpdateByUserDTO> EmployeeUpdateByUserAsync(int employeeId, EmployeeUpdateByUserDTO dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);


            var currentEmployee = await _context.Employees.SingleOrDefaultAsync(n => n.Id == employeeId);

            currentEmployee.Email = dto.Email;
            currentEmployee.Address = dto.Address;
            currentEmployee.PhoneNumber = dto.PhoneNumber;

            await _employeeRepository.Update(currentEmployee);
            await _dbu.SaveChangesAsync();

            return dto;
        }

        public async Task<int> DeleteAsync(int employeeId)
        {
            var httpContext = _httpContextAccessor.HttpContext
                 ?? throw new InvalidOperationException("There is no HttpContext in CategoryService");

            int userId = _authService.GetUserIdFromToken(httpContext);

          

            var users = await _context.UserEmployees.Where(u => u.EmployeeId == employeeId).ToListAsync();
            foreach (var user in users)
            {
                user.IsDeleted = true;
                user.UpdatedDate = DateTime.Now;
                user.UpdatedBy = userId;
            }

            var currentEmployee = await _context.Employees.SingleOrDefaultAsync(n => n.Id == employeeId);

            currentEmployee.UpdatedBy = userId;
            currentEmployee.UpdatedDate = DateTime.Now;
            currentEmployee.IsDeleted = true;

            await _dbu.SaveChangesAsync();
            return employeeId;
        }

        public async Task<EmployeeDTO> GetEmployeesAsync(int employeeId)
        {
            var employees = await _context.Employees.SingleOrDefaultAsync(n => n.Id == employeeId);

            return new EmployeeDTO //nó bị lỗi ở admin hay user vậy bạn 
            //moi lan login se bao
            //Path: /api/UserEmployee/profile
            //Authorization: No token
            {
                FullName = employees.FullName,
                Gender = employees.Gender,
                Email = employees.Email,
                PhoneNumber = employees.PhoneNumber,
                Address = employees.Address,
                JobStatus = employees.JobStatus,
                DateOfBirth = employees.DateOfBirth,
                Salary = employees.Salary
            };
        }
    }
}
