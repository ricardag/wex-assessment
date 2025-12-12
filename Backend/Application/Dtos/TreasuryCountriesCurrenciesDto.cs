using System.Text.Json.Serialization;

namespace Backend.Application.Dtos;

public record TreasuryCountriesCurrenciesDto
    {

    public record Metadata
        {
        public int Count { get; init; }

        [JsonPropertyName("total-count")]
        public int TotalCount { get; init; }

        [JsonPropertyName("total-pages")]
        public int TotalPages { get; init; }
        }

    public List<CountryCurrencyDto> Data { get; init; } = [];
    public Metadata Meta { get; init; } = new();
    }