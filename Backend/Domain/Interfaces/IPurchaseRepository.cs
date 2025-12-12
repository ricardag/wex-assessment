using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface IPurchaseRepository
    {
    Task<Purchase?> GetByIdAsync(int id);
    Task<Purchase> AddAsync(Purchase purchase);
    Task UpdateAsync(Purchase purchase);
    Task DeleteAsync(int id);
    Task<Purchase?> GetByTransactionIdentifierAsync(Guid transactionIdentifier);
    Task<(IEnumerable<Purchase> Items, int TotalCount)> GetPagedAsync(
        string? description,
        DateTime? startDate,
        DateTime? endDate,
        decimal? minAmount,
        decimal? maxAmount,
        int start,
        int pageSize);
    }
