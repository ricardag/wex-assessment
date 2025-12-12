namespace Backend.Application.Dtos;

public class PurchaseDto
    {
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime TransactionDatetimeUtc { get; init; }
    public decimal PurchaseAmount { get; init; }
    public Guid TransactionIdentifier { get; init; }
    }
