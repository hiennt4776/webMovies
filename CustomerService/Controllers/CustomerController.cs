using CustomerService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly JwtAuthService _jwtAuthService;
        public CustomerController(ICustomerService customerDetailService, JwtAuthService jwtAuthService)
        {
            _customerService = customerDetailService;
            _jwtAuthService = jwtAuthService;
        }





        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            
            var username = User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value.Trim();
            if (username == null) return Unauthorized();

            var profile = await _customerService.GetProfileAsync(username);
            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices()
        {

            return Ok(await _customerService.GetInvoicesAsync());
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
          
            return Ok(await _customerService.GetFavoriteMoviesAsync());
        }
    }
}
