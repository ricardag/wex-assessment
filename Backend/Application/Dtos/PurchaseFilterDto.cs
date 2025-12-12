namespace Backend.Application.Dtos;

public class PurchaseFilterDto
    {
    public string? Description { get; set; }
    public DateTime? TransactionStartDate { get; set; }
    public DateTime? TransactionEndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Start { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    }
