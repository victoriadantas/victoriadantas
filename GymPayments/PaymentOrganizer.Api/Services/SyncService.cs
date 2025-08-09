using Microsoft.EntityFrameworkCore;
using PaymentOrganizer.Api.Infrastructure;
using PaymentOrganizer.Api.Integrations;
using PaymentOrganizer.Api.Domain;

namespace PaymentOrganizer.Api.Services;

public class SyncService
{
    private readonly IEnumerable<IProviderClient> _clients;
    private readonly ApplicationDbContext _db;
    private readonly PaymentService _payments;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IEnumerable<IProviderClient> clients,
        ApplicationDbContext db,
        PaymentService payments,
        ILogger<SyncService> logger)
    {
        _clients = clients;
        _db = db;
        _payments = payments;
        _logger = logger;
    }

    public async Task<int> SyncMonthAsync(DateOnly month, CancellationToken ct = default)
    {
        int updated = 0;

        foreach (var client in _clients)
        {
            var statuses = await client.GetPaymentStatusesAsync(month, ct);

            foreach (var (externalId, isPaid) in statuses)
            {
                var payment = await _db.Payments.FirstOrDefaultAsync(p => p.ExternalPaymentId == externalId, ct);
                if (payment is null)
                {
                    continue;
                }

                if (isPaid && payment.Status != PaymentStatus.Paid)
                {
                    payment.Status = PaymentStatus.Paid;
                    payment.PaidOn = DateOnly.FromDateTime(DateTime.UtcNow);
                    updated++;
                }
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        await _payments.UpdateOverduesAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);
        _logger.LogInformation("Sync completed. Updated {Count} payments.", updated);
        return updated;
    }
}