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

    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly JwtAuthService _jwtAuthService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("getPageSearchInvoices")]
        public async Task<IActionResult> GetInvoices(
         [FromQuery] InvoiceQuery query)
        {
            var result = await _invoiceService.GetInvoicesAsync(query);
            return Ok(result);
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(
                int id,
                CancelInvoiceRequestDTO request)
        {
            await _invoiceService.CancelInvoiceAsync(id, request.Reason);
            return Ok();
        }


        [HttpGet("detail/{id}")]
        public async Task<ActionResult<InvoiceDTO>> GetDetail(int id)
        {
            return await _invoiceService.GetDetailAsync(id);
        }
    }
}
