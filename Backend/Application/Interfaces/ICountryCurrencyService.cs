using Backend.Application.Dtos;

namespace Backend.Application.Interfaces;

public interface ICountryCurrencyService
    {
    Task<IReadOnlyList<CountryCurrencyDto>> GetAllAsync();
    }