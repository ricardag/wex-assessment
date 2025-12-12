using Backend.Application.Configuration;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class ExchangeRateServiceTests
    {
    private readonly Mock<IHttpService> _mockHttpService;
    private readonly ExchangeRateService _sut;
    private const string BaseUrl = "https://api.treasury.gov/services/api/fiscal_service";

    public ExchangeRateServiceTests()
        {
        _mockHttpService = new Mock<IHttpService>();
        var settings = new TreasuryApiSettings { TreasuryApiBaseUrl = BaseUrl };
        _sut = new ExchangeRateService(_mockHttpService.Object, settings);
        }

    [Fact]
    public async Task GetRate_ShouldReturnExchangeRate_WhenApiReturnsData()
        {
        // Arrange
        var country = "Brazil";
        var currency = "Real";
        var date = new DateTime(2024, 12, 01);

        var expectedRate = new ExchangeRateDto
            {
            Country = country,
            Currency = currency,
            ExchangeRateRaw = "5.75",
            RecordDate = new DateTime(2024, 11, 30),
            EffectiveDate = new DateTime(2024, 11, 30)
            };

        var response = new ExchangeRateResponseDto
            {
            Data = new List<ExchangeRateDto> { expectedRate }
            };

        _mockHttpService.Setup(s => s.GetAsync<ExchangeRateResponseDto>(
                It.Is<string>(url => url.Contains(country) && url.Contains(currency)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetRate(country, currency, date);

        // Assert
        result.Should().NotBeNull();
        result!.Country.Should().Be(country);
        result.Currency.Should().Be(currency);
        result.ExchangeRate.Should().Be(5.75m);
        result.RecordDate.Should().Be(new DateTime(2024, 11, 30));

        _mockHttpService.Verify(s => s.GetAsync<ExchangeRateResponseDto>(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        }

    [Fact]
    public async Task GetRate_ShouldReturnNull_WhenApiReturnsEmptyData()
        {
        // Arrange
        var country = "NonExistent";
        var currency = "Currency";
        var date = DateTime.UtcNow;

        var response = new ExchangeRateResponseDto
            {
            Data = new List<ExchangeRateDto>()
            };

        _mockHttpService.Setup(s => s.GetAsync<ExchangeRateResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetRate(country, currency, date);

        // Assert
        result.Should().BeNull();
        _mockHttpService.Verify(s => s.GetAsync<ExchangeRateResponseDto>(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        }

    [Fact]
    public async Task GetRate_ShouldReturnNull_WhenApiReturnsNullData()
        {
        // Arrange
        var country = "Test";
        var currency = "Currency";
        var date = DateTime.UtcNow;

        var response = new ExchangeRateResponseDto
            {
            Data = null
            };

        _mockHttpService.Setup(s => s.GetAsync<ExchangeRateResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetRate(country, currency, date);

        // Assert
        result.Should().BeNull();
        }

    [Fact]
    public async Task GetRate_ShouldBuildCorrectUrl_WithEscapedParameters()
        {
        // Arrange
        var country = "Country With Spaces";
        var currency = "Currency/Symbol";
        var date = new DateTime(2024, 12, 15);
        var capturedUrl = string.Empty;

        var response = new ExchangeRateResponseDto
            {
            Data = new List<ExchangeRateDto>
                {
                new ExchangeRateDto
                    {
                    Country = country,
                    Currency = currency,
                    ExchangeRateRaw = "1.0",
                    RecordDate = date,
                    EffectiveDate = date
                    }
                }
            };

        _mockHttpService.Setup(s => s.GetAsync<ExchangeRateResponseDto>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((url, _) => capturedUrl = url)
            .ReturnsAsync(response);

        // Act
        await _sut.GetRate(country, currency, date);

        // Assert
        capturedUrl.Should().Contain(BaseUrl);
        capturedUrl.Should().Contain("2024-12-15");
        capturedUrl.Should().Contain(Uri.EscapeDataString(country));
        capturedUrl.Should().Contain(Uri.EscapeDataString(currency));
        capturedUrl.Should().Contain("sort=-record_calendar_year,-record_calendar_month,-record_calendar_day");
        capturedUrl.Should().Contain("page[size]=1");
        }
    }
