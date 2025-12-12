using Backend.Application.Dtos;

namespace Backend.Application.Interfaces;

public interface IPurchaseService
    {
    Task<PurchaseDto?> GetByIdAsync(int id);
    Task<PagedResultDto<PurchaseDto>> GetPagedAsync(PurchaseFilterDto filter);
    Task<PurchaseDto> CreateAsync(CreatePurchaseDto dto);
    Task UpdateAsync(int id, UpdatePurchaseDto dto);
    Task DeleteAsync(int id);
    Task<PurchaseDto?> GetByTransactionIdentifierAsync(Guid transactionIdentifier);
    }
