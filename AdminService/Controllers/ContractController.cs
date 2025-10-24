using AdminService.Service;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace AdminService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        public ContractController(IContractService contractService)
        {
            _contractService = contractService;

        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var result = await _contractService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy hợp đồng." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi lấy hợp đồng: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("getPageSortSearchPartners")]
        public async Task<IActionResult> GetPagedSortSearchAsync(
                 int pageNumber = 1, int pageSize = 10,
                 string? search = null, string? sortColumn = "CreateDate",
                 bool ascending = true)
        {
            var result = await _contractService.GetPagedSortSearchAsync(pageNumber, pageSize, search, sortColumn, ascending);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ContractCreateDTO contractCreateDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            Console.WriteLine($"[API] Content-Type: {Request.ContentType}");
            Console.WriteLine($"[API] HasFormContentType: {Request.HasFormContentType}");
            Console.WriteLine($"[API] Content-Length: {Request.ContentLength}");

            if (contractCreateDTO.PartnerId == null)
                return BadRequest("PartnerId không được để trống");
            // ✅ Parse JSON thủ công cho MovieInContract
            var movieJson = Request.Form["MovieInContract"];
            if (!string.IsNullOrEmpty(movieJson))
            {
                try
                {
                    contractCreateDTO.MovieInContract = JsonSerializer.Deserialize<List<MovieInContractDTO>>(movieJson!);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API] JSON parse error: {ex.Message}");
                    return BadRequest("Invalid MovieInContract format.");
                }
            }


            var result = await _contractService.CreateContractAsync(contractCreateDTO);
         
            return Ok(new
            {
                Success = true,
                Message = "Contract created successfully.",
                //ContractId = contract.Id
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ContractEditDTO contractEditDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            Console.WriteLine($"[API] Content-Type: {Request.ContentType}");
            Console.WriteLine($"[API] HasFormContentType: {Request.HasFormContentType}");
            Console.WriteLine($"[API] Content-Length: {Request.ContentLength}");

            if (contractEditDTO.PartnerId == null)
                return BadRequest("PartnerId không được để trống");
            // ✅ Parse JSON thủ công cho MovieInContract
            var movieJson = Request.Form["MovieInContract"];
            if (!string.IsNullOrEmpty(movieJson))
            {
                try
                {
                    contractEditDTO.MovieInContract = JsonSerializer.Deserialize<List<MovieInContractDTO>>(movieJson!);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API] JSON parse error: {ex.Message}");
                    return BadRequest("Invalid MovieInContract format.");
                }
            }


            var result = await _contractService.UpdateContractAsync(contractEditDTO);

            return Ok(new
            {
                Success = true,
                Message = "Contract created successfully.",
                //ContractId = contract.Id
            });
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _contractService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }


        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _contractService.DownloadFileWithMetaAsync(id);
            if (result == null)
                return NotFound("File không tồn tại.");

            var (fileBytes, fileName, contentType) = result.Value;

            return File(fileBytes, contentType, fileName);
        }
    }

}
