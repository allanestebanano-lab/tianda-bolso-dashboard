using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Reports.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin,Gerente")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var result = await _service.GetKpisAsync();
            return Ok(result);
        }

        [HttpGet("ventas-por-dia")]
        public async Task<IActionResult> GetDailySales()
        {
            var result = await _service.GetDailySalesAsync();
            return Ok(result);
        }

        [HttpGet("top-3-productos")]
        public async Task<IActionResult> GetTopProducts()
        {
            var result = await _service.GetTopProductsAsync();
            return Ok(result);
        }

        [HttpGet("ventas-por-categoria")]
        public async Task<IActionResult> GetSalesByCategory()
        {
            var result = await _service.GetSalesByCategoryAsync();
            return Ok(result);
        }

        [HttpGet("estado-inventario")]
        public async Task<IActionResult> GetInventoryStatus()
        {
            var result = await _service.GetInventoryStatusAsync();
            return Ok(result);
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<IActionResult> GetMostSoldProducts()
        {
            var result = await _service.GetMostSoldProductsAsync();
            return Ok(result);
        }
    }
}
