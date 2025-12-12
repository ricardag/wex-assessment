using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Interfaces;

namespace Backend.Application.Services;

public class PurchaseService : IPurchaseService
    {
    private readonly IPurchaseRepository _purchaseRepository;

    public PurchaseService(IPurchaseRepository purchaseRepository)
        {
        _purchaseRepository = purchaseRepository;
        }

    public async Task<PurchaseDto?> GetByIdAsync(int id)
        {
        var purchase = await _purchaseRepository.GetByIdAsync(id);

        return purchase == null ? null : MapToDto(purchase);
        }

    public async Task<PagedResultDto<PurchaseDto>> GetPagedAsync(PurchaseFilterDto filter)
        {
        var (items, totalCount) = await _purchaseRepository.GetPagedAsync(
            filter.Description,
            filter.TransactionStartDate,
            filter.TransactionEndDate,
            filter.MinAmount,
            filter.MaxAmount,
            filter.Start,
            filter.PageSize);

        var dtos = items.Select(MapToDto);

        return new PagedResultDto<PurchaseDto>
            {
            Items = dtos,
            Count = totalCount,
            };
        }

    public async Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto)
        {
        var purchase = new Purchase
            {
            Description = dto.Description,
            TransactionDatetimeUtc = dto.TransactionDateUtc,
            PurchaseAmount = dto.PurchaseAmount,
            TransactionIdentifier = Guid.NewGuid()
            };

        var createdPurchase = await _purchaseRepository.AddAsync(purchase);
        return MapToDto(createdPurchase);
        }

    public async Task UpdateAsync(int id, UpdatePurchaseDto dto)
        {
        var purchase = await _purchaseRepository.GetByIdAsync(id);

        if (purchase == null)
            throw new KeyNotFoundException($"Purchase with ID {id} not found");

        purchase.Description = dto.Description;
        purchase.PurchaseAmount = dto.PurchaseAmount;
        purchase.TransactionDatetimeUtc = dto.TransactionDateUtc;

        await _purchaseRepository.UpdateAsync(purchase);
        }

    public async Task DeleteAsync(int id)
        {
        var purchase = await _purchaseRepository.GetByIdAsync(id);

        if (purchase == null)
            throw new KeyNotFoundException($"Purchase with ID {id} not found");

        await _purchaseRepository.DeleteAsync(id);
        }

    public async Task<PurchaseDto?> GetByTransactionIdentifierAsync(Guid transactionIdentifier)
        {
        var purchase = await _purchaseRepository.GetByTransactionIdentifierAsync(transactionIdentifier);

        if (purchase == null)
            return null;

        return MapToDto(purchase);
        }

    private static PurchaseDto MapToDto(Purchase purchase)
        {
        return new PurchaseDto
            {
            Id = purchase.Id,
            Description = purchase.Description,
            TransactionDatetimeUtc = purchase.TransactionDatetimeUtc,
            PurchaseAmount = purchase.PurchaseAmount,
            TransactionIdentifier = purchase.TransactionIdentifier
            };
        }
    }
