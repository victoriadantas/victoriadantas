namespace PaymentOrganizer.Api.Integrations;

public class GympassClient : IProviderClient
{
    private readonly ILogger<GympassClient> _logger;

    public string Name => "Gympass";

    public GympassClient(ILogger<GympassClient> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<(string externalPaymentId, bool isPaid)>> GetPaymentStatusesAsync(
        DateOnly month,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Simulating Gympass API sync for {Month}", month);
        return Task.FromResult<IReadOnlyList<(string, bool)>>(Array.Empty<(string, bool)>());
    }
}