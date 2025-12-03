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
    public class SubscriptionPackageController : ControllerBase
    {
        private readonly ISubscriptionPackageService _subscriptionPackageService;
        public SubscriptionPackageController(ISubscriptionPackageService service) {
            _subscriptionPackageService = service;

        }
        // GET: api/SubscriptionPackages?search=...&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _subscriptionPackageService.GetAllPagedAsync(search, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _subscriptionPackageService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionPackageDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _subscriptionPackageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubscriptionPackageDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id && dto.Id != 0) return BadRequest("Id mismatch");
            var updated = await _subscriptionPackageService.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _subscriptionPackageService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
