using CustomerService.Service;
using dbMovies.Models;
using helperMovies.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPackageController : ControllerBase
    {
        private readonly ISubscriptionPackageService _subscriptionPackageService;
        public SubscriptionPackageController(ISubscriptionPackageService subscriptionPackageService)
        {
            _subscriptionPackageService = subscriptionPackageService;

        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var dto = await _subscriptionPackageService.GetAllAsync();

            return Ok(await _subscriptionPackageService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dto = await _subscriptionPackageService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
        [Authorize]
        [HttpPost("purchase/package")]
        public async Task<IActionResult> Purchase(int packageId)
        {
            var invoiceId = await _subscriptionPackageService.PurchaseAsync(packageId);

            return Ok(invoiceId);
        }
        //[Authorize]
        //[HttpPost("purchase/package")]
        //public async Task<IActionResult> Purchase(PurchasePackageRequest req)
        //{
        //    var invoiceId = await _purchaseService.PurchaseAsync(CurrentUserId, req.PackageId);

        //    return Ok(invoiceId);
        //}

        [Authorize]
        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromQuery] int invoiceId,
                                                  [FromQuery] bool success)
        {
            await _subscriptionPackageService.SimulatePaymentAsync(invoiceId, success);
            return Ok();
        }


    }
}
