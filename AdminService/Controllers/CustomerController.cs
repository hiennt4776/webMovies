using AdminService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Attributes;
using System.Threading.Tasks;


namespace AdminService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly JwtAuthService _jwtAuthService;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet("getPageSearchCustomer")]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] string? keyword,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetCustomersAsync(
                keyword, pageIndex, pageSize);
            return Ok(result);
        }
    }
}
