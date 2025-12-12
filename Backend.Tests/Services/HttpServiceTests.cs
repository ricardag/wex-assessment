using System.Net;
using System.Text.Json;
using Backend.Application.Services;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Services;

public class HttpServiceTests
    {
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly HttpService _sut;

    public HttpServiceTests()
        {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _sut = new HttpService(_httpClient);
        }

    [Fact]
    public async Task GetAsync_ShouldReturnDeserializedObject_WhenResponseIsSuccessful()
        {
        // Arrange
        var url = "https://api.example.com/data";
        var testData = new TestDto { Id = 1, Name = "Test" };
        var jsonContent = JsonSerializer.Serialize(testData, new JsonSerializerOptions
            {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent)
                });

        // Act
        var result = await _sut.GetAsync<TestDto>(url);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
        result.Name.Should().Be(testData.Name);
        }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenResponseIs404NotFound()
        {
        // Arrange
        var url = "https://api.example.com/notfound";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.NotFound
                });

        // Act
        var result = await _sut.GetAsync<TestDto>(url);

        // Assert
        result.Should().BeNull();
        }

    [Fact]
    public async Task GetAsync_ShouldThrowUnauthorizedAccessException_WhenResponseIs401()
        {
        // Arrange
        var url = "https://api.example.com/secured";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.Unauthorized
                });

        // Act
        var act = async () => await _sut.GetAsync<TestDto>(url);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"Unauthorized access to {url}");
        }

    [Fact]
    public async Task GetAsync_ShouldThrowHttpRequestException_WhenResponseIs500()
        {
        // Arrange
        var url = "https://api.example.com/error";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.InternalServerError
                });

        // Act
        var act = async () => await _sut.GetAsync<TestDto>(url);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage($"Request to {url} failed with status code {HttpStatusCode.InternalServerError}");
        }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenResponseContentIsEmpty()
        {
        // Arrange
        var url = "https://api.example.com/empty";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
                });

        // Act
        var result = await _sut.GetAsync<TestDto>(url);

        // Assert
        result.Should().BeNull();
        }

    [Fact]
    public async Task GetAsync_ShouldThrowTimeoutException_WhenRequestTimesOut()
        {
        // Arrange
        var url = "https://api.example.com/timeout";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var act = async () => await _sut.GetAsync<TestDto>(url);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage($"Request to {url} timed out");
        }

    [Fact]
    public async Task GetAsync_ShouldThrowInvalidOperationException_WhenJsonIsInvalid()
        {
        // Arrange
        var url = "https://api.example.com/badjson";
        var invalidJson = "{ invalid json }";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
                {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(invalidJson)
                });

        // Act
        var act = async () => await _sut.GetAsync<TestDto>(url);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to deserialize response from {url}");
        }

    // Helper DTO for testing
    private class TestDto
        {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        }
    }
