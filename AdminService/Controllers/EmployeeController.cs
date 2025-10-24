using AdminService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly JwtAuthService _jwtAuthService;
        public EmployeeController(IEmployeeService employeeService, JwtAuthService jwtAuthService)
        {
            _employeeService = employeeService;
            _jwtAuthService = jwtAuthService;
        }


        [HttpGet]
        [Route("getAllEmployees")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetEmployees();
            return Ok(result);
        }



        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value.Trim();
            if (username == null) return Unauthorized();

            var profile = await _employeeService.GetProfileAsync(username);
            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetEmployeesFilterAsync([FromQuery] EmployeeFilterDTO filter)
        {
            var result = await _employeeService.GetEmployeesFilterAsync(filter);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddEmployeeAsync(EmployeeCreateDTO employeeCreateDTO)
        {
            var result = await _employeeService.AddEmployeeAsync(employeeCreateDTO);
            return Ok(result);
        }

        [HttpPut("update/{employeeId}")]
        public async Task<IActionResult> EditEmployeeAsync(int employeeId, EmployeeUpdateDTO employeeUpdateDTO)
        {
            var result = await _employeeService.EditEmployeeAsync(employeeId, employeeUpdateDTO);
            return Ok(result);
        }


        [HttpDelete("delete/{employeeId}")]

        public async Task<IActionResult> Delete(int employeeId)
        {

            await _employeeService.DeleteAsync(employeeId);

            return Ok();
        }

        [HttpGet("get/{employeeId}")]

        public async Task<IActionResult> GetEmployeeAsync(int employeeId)
        {

            var result = await _employeeService.GetEmployeesAsync(employeeId);

            return Ok(result);
        }

    }
}
