using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaBolsos.Core.Modules.Analytics.Interfaces;

namespace TiendaBolsos.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,Gerente")]
public sealed class AnalyticsController(IAnalyticsService service) : ControllerBase
{
    [HttpGet("business")]
    public async Task<IActionResult> GetBusiness(CancellationToken cancellationToken) =>
        Ok(await service.GetBusinessDashboardAsync(cancellationToken));

    [HttpGet("technical")]
    public async Task<IActionResult> GetTechnical(CancellationToken cancellationToken) =>
        Ok(await service.GetTechnicalDashboardAsync(cancellationToken));
}
