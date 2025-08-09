namespace PaymentOrganizer.Api.Domain;

public enum ProviderType
{
    Gympass = 1,
    TotalPass = 2
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}