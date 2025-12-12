using System.Text.Json.Serialization;
using System.Globalization;

namespace Backend.Application.Dtos;

public record ExchangeRateDto
    {
    // Remaining members from the Treasury API are not required
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    // We need this because "exchange_rate" is a string, but we need the decimal value
    [JsonPropertyName("exchange_rate")]
    public string ExchangeRateRaw { get; set; } = string.Empty;

    public decimal ExchangeRate => decimal.Parse(ExchangeRateRaw, CultureInfo.InvariantCulture);

    [JsonPropertyName("record_date")]
    public DateTime RecordDate { get; set; }

    [JsonPropertyName("effective_date")]
    public DateTime EffectiveDate { get; set; }
    }

public record ExchangeRateResponseDto
    {
    // Remaining members from the Treasury API are not required
    public List<ExchangeRateDto>? Data { get; set; }
    }