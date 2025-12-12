using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using Backend.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data.Repositories;

public class PurchaseRepository : IPurchaseRepository
    {
    private readonly AppDbContext _context;

    public PurchaseRepository(AppDbContext context)
        {
        _context = context;
        }

    public async Task<Purchase?> GetByIdAsync(int id)
        {
        return await _context.Purchases.FindAsync(id);
        }

    public async Task<Purchase> AddAsync(Purchase purchase)
        {
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();
        return purchase;
        }

    public async Task UpdateAsync(Purchase purchase)
        {
        _context.Purchases.Update(purchase);
        await _context.SaveChangesAsync();
        }

    public async Task DeleteAsync(int id)
        {
        var purchase = await GetByIdAsync(id);
        if (purchase != null)
            {
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
            }
        }

    public async Task<Purchase?> GetByTransactionIdentifierAsync(Guid transactionIdentifier)
        {
        return await _context.Purchases
            .FirstOrDefaultAsync(p => p.TransactionIdentifier == transactionIdentifier);
        }

    public async Task<(IEnumerable<Purchase> Items, int TotalCount)> GetPagedAsync(
        string? description,
        DateTime? startDate,
        DateTime? endDate,
        decimal? minAmount,
        decimal? maxAmount,
        int start,
        int pageSize)
        {
        var query = _context.Purchases.AsQueryable();

        if (!string.IsNullOrWhiteSpace(description))
            query = query.Where(p => EF.Functions.Like(p.Description, $"%{description}%"));

        if (startDate.HasValue)
            query = query.Where(p => p.TransactionDatetimeUtc >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.TransactionDatetimeUtc <= endDate.Value);

        if (minAmount.HasValue)
            query = query.Where(p => p.PurchaseAmount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(p => p.PurchaseAmount <= maxAmount.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.TransactionDatetimeUtc)
            .Skip(start)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
        }
    }
