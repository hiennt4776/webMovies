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

    public class MoviePricingController : ControllerBase
    {
        private readonly IMoviePricingService _moviePricingService;
        public MoviePricingController(IMoviePricingService moviePricingService)
        {
            _moviePricingService = moviePricingService;
        }

        [HttpGet("rent/{movieId}")]
        public async Task<IActionResult> GetRentPrices(int movieId)
        {
            return Ok(await _moviePricingService.GetRentPricesAsync(movieId));
        }

        [HttpGet("buy/{movieId}")]
        public async Task<IActionResult> GetBuyPrice(int movieId)
        {
            return Ok(await _moviePricingService.GetBuyPriceAsync(movieId));
        }

    }
}
