using SalamHack.Domain.Services;

namespace SalamHack.Application.Common.Interfaces;

public interface IServiceHistoryAnalyzer
{
    Task<ServiceHistoryStats> AnalyzeAsync(
        Guid userId,
        Guid serviceId,
        CancellationToken cancellationToken = default);
}
