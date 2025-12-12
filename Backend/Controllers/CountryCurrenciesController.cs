using System.Net;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

public class CountryCurrenciesController : ApiController
    {
    private readonly ICountryCurrencyService _countryCurrencyService;
    private readonly IExchangeRateService _exchangeRateService;

    public CountryCurrenciesController(ICountryCurrencyService countryCurrencyService, IExchangeRateService exchangeRateService)
        {
        _countryCurrencyService = countryCurrencyService;
        _exchangeRateService = exchangeRateService;
        }

    [Route("country-currencies"), HttpGet]
    [ProducesResponseType(typeof(List<CountryCurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
        {
        var result = await _countryCurrencyService.GetAllAsync();
        return Ok(result);
        }

    [Route("country-currencies/{country}/{currency}/{date:datetime}"), HttpGet]
    [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string country, string currency, DateTime date)
        {
        var result = await _exchangeRateService.GetRate(country, currency, date);
        return Ok(result);
        }
    }