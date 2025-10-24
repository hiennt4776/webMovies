using AdminService.Service;
using helperMovies.constMovies;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]


    public class PartnerController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnerController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        [Route("getAllPartners")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _partnerService.GetAllAsync();
            return Ok(result);
        }


        [HttpGet]
        [Route("getAllPartnersByStatus")]
        public async Task<IActionResult> GetPartnerByStatus(string partnerStatusConstant)
        {
            var result = await _partnerService.GetPartnerByStatus(partnerStatusConstant);
            return Ok(result);
        }

        [HttpGet]
        [Route("getPagePartners")]
        public async Task<ActionResult<PagedResult<PartnerDTO>>> GetPaged(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _partnerService.GetPagedAsync(pageNumber, pageSize);
            return Ok(result);
        }


        [HttpGet]
        [Route("getPageSortSearchPartners")]
        public async Task<IActionResult> GetPagedSortSearchAsync(
            int pageNumber = 1, int pageSize = 10,
            string? search = null, string? sortColumn = "CreateDate",
            bool ascending = true, string? status = null)
        {
            var result = await _partnerService.GetPagedSortSearchAsync(pageNumber, pageSize, search, sortColumn, ascending, status);
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _partnerService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Add([FromBody] PartnerDTO dto)
        {
            var result = await _partnerService.AddAsync(dto);
            return Ok(result);
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PartnerDTO dto)
        {
            var updated = await _partnerService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _partnerService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
