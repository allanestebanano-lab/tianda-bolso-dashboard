using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Inventory.DTOs;
using TiendaBolsos.Core.Modules.Inventory.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gerente")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var inventories = await _service.GetAllAsync();
            return Ok(inventories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var inventory = await _service.GetByIdAsync(id);

            if (inventory is null)
            {
                return NotFound();
            }

            return Ok(inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInventoryDto dto)
        {
            var inventory = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = inventory.IdInventory }, inventory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateInventoryDto dto)
        {
            var inventory = await _service.UpdateAsync(id, dto);
            return Ok(inventory);
        }
    }
}
