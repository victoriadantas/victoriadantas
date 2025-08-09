using Quartz;
using PaymentOrganizer.Api.Services;

namespace PaymentOrganizer.Api.Jobs;

public class DailySyncJob : IJob
{
    private readonly SyncService _syncService;
    private readonly ILogger<DailySyncJob> _logger;

    public DailySyncJob(SyncService syncService, ILogger<DailySyncJob> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var month = new DateOnly(now.Year, now.Month, 1);
        _logger.LogInformation("Running DailySyncJob for {Month}", month);
        await _syncService.SyncMonthAsync(month, context.CancellationToken);
    }
}