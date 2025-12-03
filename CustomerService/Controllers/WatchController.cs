using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.EntityFrameworkCore;
using CustomerService.Service;

namespace CustomerService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class WatchController : ControllerBase
    {
        private readonly IWatchHistoryService _watchHistoryService;
        private readonly IUserCustomerService _userCustomerService;

        public WatchController(IWatchHistoryService watchHistoryService, IUserCustomerService userCustomerService)
        {
            _watchHistoryService = watchHistoryService;
            _userCustomerService = userCustomerService;
        }

        // GET
        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetPosition(int movieId)
        {

            var pos = await _watchHistoryService.GetWatchTimeAsync(movieId);
            return Ok(pos);
        }

        // POST
        [HttpPost("update")]
        public async Task<IActionResult> UpdatePosition([FromBody] WatchHistoryDTO req)
        {

            await _watchHistoryService.SaveWatchTimeAsync(req);
            return Ok();
        }
    }
}
