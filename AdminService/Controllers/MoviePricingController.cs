using AdminService.Service;
using helperMovies.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviePricingController : ControllerBase
    {
        private readonly IMoviePricingService _service;
        public MoviePricingController(IMoviePricingService service) => _service = service;

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetByMovie(int movieId)
        {
            var list = await _service.GetByMovieAsync(movieId);
            return Ok(list);
        }

        [HttpGet("movie/{movieId}/active")]
        public async Task<IActionResult> GetActive(int movieId)
        {
            var price = await _service.GetActivePriceAsync(movieId, DateTime.UtcNow);
            if (price == null) return NotFound();
            return Ok(price);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUpdateMoviePricingDTO dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateMoviePricingDTO dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.SoftDeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Rent endpoint (sample)
        [HttpPost("{id}/rent")]
        public async Task<IActionResult> Rent(int id)
        {
            var expiry = await _service.RentAsync(id);
            if (expiry == null) return BadRequest(new { message = "Cannot rent with this pricing id" });
            // return expiry UTC
            return Ok(new { rentalExpiryUtc = expiry.Value });
        }
    }
}