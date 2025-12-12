using Backend.Application.Dtos;
using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ICountryCurrencyRepository
    {
    Task<List<CountryCurrency>> GetAllAsync();
    Task CreateOrUpdateAllAsync(List<CountryCurrencyDto> currencies);
    }