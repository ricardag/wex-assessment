using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Domain.Interfaces;
using System.Linq;

namespace Backend.Application.Services;

public class CountryCurrencyService : ICountryCurrencyService
    {
    private readonly ICountryCurrencyRepository _countryCurrencyRepository;

    public CountryCurrencyService(ICountryCurrencyRepository countryCurrencyRepository)
        {
        _countryCurrencyRepository = countryCurrencyRepository;
        }

    public async Task<IReadOnlyList<CountryCurrencyDto>> GetAllAsync()
        {
        var data = await _countryCurrencyRepository
            .GetAllAsync();

        var list = data
            .Select(p => new CountryCurrencyDto()
                {
                Id = p.Id,
                Country = p.Country,
                Currency = p.Currency
                })
            .ToList();

        return list;
        }
    }