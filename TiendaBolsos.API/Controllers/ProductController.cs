using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Products.DTOs;
using TiendaBolsos.Core.Modules.Products.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    [Authorize(Roles = "Admin,Gerente,Cajero")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(
            IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _service.GetAllAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _service.GetByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var product = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(Get), new { id = product.IdProducto }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            var product = await _service.UpdateAsync(id, dto);

            return Ok(product);
        }
    }
}
