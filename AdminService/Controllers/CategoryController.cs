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

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        [HttpGet]
        [Route("getAllCategories")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("getPageCategories")]
        public async Task<ActionResult<PagedResult<CategoryDTO>>> GetPaged(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _categoryService.GetPagedAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet]
        [Route("getPageSortSearchCategories")]
        public async Task<ActionResult<PagedResult<CategoryDTO>>> GetPagedSortSearchAsync(
              int pageNumber = 1,
              int pageSize = 10,
              string? search = null,
              string? sortColumn = "CreateDate",
              bool ascending = true)
        {
            var result = await _categoryService.GetPagedSortSearchAsync(pageNumber, pageSize, search, sortColumn, ascending);
            return Ok(result);
        }


        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            var result = await _categoryService.AddAsync(dto);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, CategoryDTO dto)
        {
            var result = await _categoryService.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
