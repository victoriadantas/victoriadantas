namespace PaymentOrganizer.Api.Integrations;

public class TotalPassClient : IProviderClient
{
    private readonly ILogger<TotalPassClient> _logger;

    public string Name => "TotalPass";

    public TotalPassClient(ILogger<TotalPassClient> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<(string externalPaymentId, bool isPaid)>> GetPaymentStatusesAsync(
        DateOnly month,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Simulating TotalPass API sync for {Month}", month);
        return Task.FromResult<IReadOnlyList<(string, bool)>>(Array.Empty<(string, bool)>());
    }
}