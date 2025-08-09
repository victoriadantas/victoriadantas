using Quartz;
using PaymentOrganizer.Api.Services;

namespace PaymentOrganizer.Api.Jobs;

public class MonthlyBillingJob : IJob
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<MonthlyBillingJob> _logger;

    public MonthlyBillingJob(PaymentService paymentService, ILogger<MonthlyBillingJob> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var target = new DateOnly(now.Year, now.Month, 1);
        _logger.LogInformation("Running MonthlyBillingJob for {Year}-{Month}", target.Year, target.Month);
        await _paymentService.GenerateMonthlyPaymentsAsync(target, context.CancellationToken);
    }
}