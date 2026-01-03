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

    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceRequestDTO request)
        {
    

            var invoiceId = await _invoiceService.CreateInvoiceAsync(
                request.MoviePricingId);

            return Ok(invoiceId);
        }

    }
}
