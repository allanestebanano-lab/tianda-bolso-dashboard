using TiendaBolsos.Core.Modules.Analytics.DTOs;

namespace TiendaBolsos.Core.Modules.Analytics.Interfaces;

public interface IAnalyticsRepository
{
    Task<BusinessDashboardDto> GetBusinessDashboardAsync(CancellationToken cancellationToken);

    Task<TechnicalDashboardDto> GetTechnicalDashboardAsync(CancellationToken cancellationToken);
}
