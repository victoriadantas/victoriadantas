using Microsoft.EntityFrameworkCore;
using PaymentOrganizer.Api.Domain;
using PaymentOrganizer.Api.Domain.Entities;
using PaymentOrganizer.Api.Infrastructure;

namespace PaymentOrganizer.Api.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ApplicationDbContext db, ILogger<PaymentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> GenerateMonthlyPaymentsAsync(DateOnly periodStart, CancellationToken ct = default)
    {
        int year = periodStart.Year;
        int month = periodStart.Month;

        var activeSubs = await _db.Subscriptions.Where(s => s.IsActive).ToListAsync(ct);
        int created = 0;

        foreach (var sub in activeSubs)
        {
            bool exists = await _db.Payments.AnyAsync(p => p.SubscriptionId == sub.Id && p.Year == year && p.Month == month, ct);
            if (exists)
            {
                continue;
            }

            int billingDay = Math.Clamp(sub.BillingDay, 1, DateTime.DaysInMonth(year, month));
            var due = new DateOnly(year, month, billingDay);

            var payment = new Payment
            {
                SubscriptionId = sub.Id,
                Year = year,
                Month = month,
                DueDate = due,
                Amount = sub.MonthlyPrice,
                Status = PaymentStatus.Pending
            };

            _db.Payments.Add(payment);
            created++;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Generated {Count} payments for {Year}-{Month}", created, year, month);
        return created;
    }

    public async Task<int> UpdateOverduesAsync(DateOnly today, CancellationToken ct = default)
    {
        var pending = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < today)
            .ToListAsync(ct);

        foreach (var p in pending)
        {
            p.Status = PaymentStatus.Overdue;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Marked {Count} payments as overdue", pending.Count);
        return pending.Count;
    }

    public async Task<int> MarkAsPaidAsync(int paymentId, DateOnly paidOn, CancellationToken ct = default)
    {
        var payment = await _db.Payments.FindAsync(new object[] { paymentId }, ct);
        if (payment is null)
        {
            return 0;
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaidOn = paidOn;
        await _db.SaveChangesAsync(ct);
        return 1;
    }
}