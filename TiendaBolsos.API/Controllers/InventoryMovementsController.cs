using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Inventory.DTOs;
using TiendaBolsos.Core.Modules.Inventory.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gerente")]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryMovementsController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var movements = await _service.GetMovementsAsync();
            return Ok(movements);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInventoryMovementDto dto)
        {
            var movement = await _service.CreateMovementAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = movement.IdMovement }, movement);
        }
    }
}
