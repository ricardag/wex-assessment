using Backend.Application.Dtos;

namespace Backend.Application.Interfaces;

public interface IExchangeRateService
    {
    Task<ExchangeRateDto?> GetRate(string countryCode, string currency, DateTime date);
    }