using PaymentOrganizer.Api.Domain;

namespace PaymentOrganizer.Api.Domain.Entities;

public class Subscription
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public ProviderType Provider { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public decimal MonthlyPrice { get; set; }

    public int BillingDay { get; set; } = 1; // 1-28/31 ajustado no serviço

    public DateOnly StartedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public List<Payment> Payments { get; set; } = new();
}