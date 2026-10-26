using TiendaBolsos.Core.Modules.Analytics.DTOs;

namespace TiendaBolsos.Core.Modules.Analytics.Interfaces;

public interface IAnalyticsService
{
    Task<BusinessDashboardDto> GetBusinessDashboardAsync(CancellationToken cancellationToken);

    Task<TechnicalDashboardDto> GetTechnicalDashboardAsync(CancellationToken cancellationToken);
}
