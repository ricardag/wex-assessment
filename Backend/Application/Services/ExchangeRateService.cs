using Backend.Application.Configuration;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Serilog;

namespace Backend.Application.Services;

public class ExchangeRateService : IExchangeRateService
    {
    private readonly IHttpService _httpService;
    private readonly string _treasuryApiBaseUrl;
    
    public ExchangeRateService(IHttpService httpService, TreasuryApiSettings treasuryApiSettings)
        {
        _httpService = httpService;
        _treasuryApiBaseUrl = treasuryApiSettings.TreasuryApiBaseUrl;
        }
    

    public async Task<ExchangeRateDto?> GetRate(string country, string currency, DateTime date)
        {
        var dateFormatted = date.ToString("yyyy-MM-dd");
        var countryEscaped = Uri.EscapeDataString(country);
        var currencyEscaped = Uri.EscapeDataString(currency);

        var url = $"{_treasuryApiBaseUrl}?filter=record_date:lte:{dateFormatted}," +
                  $"country:eq:{countryEscaped}," +
                  $"currency:eq:{currencyEscaped}" +
                  $"&sort=-record_calendar_year,-record_calendar_month,-record_calendar_day&page[size]=1";
        
        var response = await _httpService.GetAsync<ExchangeRateResponseDto>(url);

        if (response?.Data == null || response.Data.Count == 0)
            {
            Log.Warning("Empty response from Treasury API for {Country} and {Currency}", country, currency);
            return null;
            }

        return response.Data.First();
        }
    }