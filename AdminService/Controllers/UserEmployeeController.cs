using AdminService.Attributes;
using AdminService.Service;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserEmployeeController : ControllerBase
    {
        private readonly  IUserEmployeeService _userEmployeeService;
        private readonly JwtAuthService _jwtAuthService;
        public UserEmployeeController(IUserEmployeeService userEmployeeService, JwtAuthService jwtAuthService)
        {
            _userEmployeeService = userEmployeeService;
            _jwtAuthService = jwtAuthService;
        }



        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginEmployeeRequestDTO model)
        {
            int k = 1;
            var res = await _userEmployeeService.Login(model);
            if (res == new LoginEmployeeResponseDTO { messenger = MessageLogin.UserNotFound })
            {
                return NotFound(res);
            }
            else if (res == new LoginEmployeeResponseDTO { messenger = MessageLogin.PasswordIncorrect })
            {
                return Unauthorized(res);
            }
            else if (res ==  new LoginEmployeeResponseDTO { messenger = MessageLogin.ErrorInServer })
            {
                return StatusCode(500, res);
            }
            return Ok(res);
        }
        [Authorize]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
                   string? keyword,
                int? roleId,
                  bool? isLocked,
                  int page = 1,
                    int pageSize = 10)
        {
            var result = await _userEmployeeService.GetPagedAsync(keyword, roleId, isLocked, page, pageSize);
            return Ok(result);
        }

        [HttpPost("create")]
        [Authorization("RoleBasedPolicy", "ADMIN")]
        public async Task<IActionResult> Create(UserEmployeeCreateDTO dto)
        {
            
            var result = await _userEmployeeService.CreateAsync(dto);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]

        [HttpPut("changepassword/{employeeId}")]
        public async Task<IActionResult> ChangePassword(int employeeId, ChangePasswordDTO dto)
        {
            var result = await _userEmployeeService.ChangePasswordAsync(dto);

            if (result == null) return NotFound();
            return Ok(result);

        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{employeeId}")]
        public async Task<IActionResult> EditEmployeeAsync(int employeeId, UserEmployeeUpdateDTO employeeUpdateDTO)
        {
            var result = await _userEmployeeService.UpdateAsync(employeeUpdateDTO);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("resetpassword/{employeeId}")]

        public async Task<IActionResult> ResetPasswordAsync(int employeeId, UserEmployeeResetPasswordDTO userEmployeeResetPasswordDTO)
        {
            var result = await _userEmployeeService.ResetPasswordAsync(userEmployeeResetPasswordDTO);
            return Ok(result);
        }

        [HttpGet("get/{id}")]

        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var user = await _userEmployeeService.GetUserEmployeesAsync(id);
            
            return Ok(user);
        }


        [HttpGet("getEdit/{id}")]
     
        public async Task<IActionResult> GetByEditIdAsync(int id)
        {
            var user = await _userEmployeeService.GetUserEditEmployeesAsync(id);

            return Ok(user);
        }
    }
}
