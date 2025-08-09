using PaymentOrganizer.Api.Domain;

namespace PaymentOrganizer.Api.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateOnly? PaidOn { get; set; }

    public string? ExternalPaymentId { get; set; }
}