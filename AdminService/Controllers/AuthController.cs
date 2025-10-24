using helperMovies.DTO;
using dbMovies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AdminService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtAuthService _jwtAuthService;
        private readonly dbMoviesContext _context;

        public AuthController(JwtAuthService jwtAuthService, dbMoviesContext context)
        {
            _jwtAuthService = jwtAuthService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginEmployeeRequestDTO login)
        {
            var user = await _context.UserEmployees
                .FirstOrDefaultAsync(u => u.Username == login.Username && u.PasswordHash == login.Password);

            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            var token = _jwtAuthService.GenerateToken(user);
            return Ok(new { token });
        }
    }

}
