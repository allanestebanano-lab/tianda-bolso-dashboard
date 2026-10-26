using TiendaBolsos.Core.Modules.Analytics.DTOs;
using TiendaBolsos.Core.Modules.Analytics.Interfaces;

namespace TiendaBolsos.Core.Modules.Analytics.Services;

public sealed class AnalyticsService(IAnalyticsRepository repository) : IAnalyticsService
{
    public Task<BusinessDashboardDto> GetBusinessDashboardAsync(
        CancellationToken cancellationToken) =>
        repository.GetBusinessDashboardAsync(cancellationToken);

    public Task<TechnicalDashboardDto> GetTechnicalDashboardAsync(
        CancellationToken cancellationToken) =>
        repository.GetTechnicalDashboardAsync(cancellationToken);
}
