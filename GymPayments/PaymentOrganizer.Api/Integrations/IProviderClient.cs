namespace PaymentOrganizer.Api.Integrations;

public interface IProviderClient
{
    string Name { get; }

    Task<IReadOnlyList<(string externalPaymentId, bool isPaid)>> GetPaymentStatusesAsync(
        DateOnly month,
        CancellationToken ct = default);
}