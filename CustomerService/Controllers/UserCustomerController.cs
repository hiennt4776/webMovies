using CustomerService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserCustomerController : Controller
    {
        private readonly IUserCustomerService _userCustomerService;

        public UserCustomerController(IUserCustomerService userCustomerService)
        {
            _userCustomerService = userCustomerService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterDTO dto)
        {
            try
            {
                await _userCustomerService.RegisterAsync(dto);
                return Ok(new { success = true, message = "Đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCustomerRequestDTO req)
        {
            var result = await _userCustomerService.LoginAsync(req);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }
    }
}
