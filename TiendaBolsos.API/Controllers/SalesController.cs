using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Sales.DTOs;
using TiendaBolsos.Core.Modules.Sales.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]
    [Route("api/ventas")]
    [Authorize(Roles = "Admin,Gerente,Cajero")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _service;

        public SalesController(ISalesService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var sales = await _service.GetAllAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var sale = await _service.GetByIdAsync(id);

            if (sale is null)
            {
                return NotFound();
            }

            return Ok(sale);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSaleDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var sale = await _service.CreateAsync(userId, dto);
                return CreatedAtAction(nameof(Get), new { id = sale.IdVenta }, sale);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
