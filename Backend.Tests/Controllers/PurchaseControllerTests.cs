using System.Net;
using System.Net.Http.Json;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class PurchaseControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IPurchaseService> _mockPurchaseService;

    public PurchaseControllerTests(WebApplicationFactory<Program> factory)
        {
        _mockPurchaseService = new Mock<IPurchaseService>();

        _factory = factory.WithWebHostBuilder(builder =>
            {
            // Set required environment variables for testing
            builder.UseSetting("JWT:Issuer", "TestIssuer");
            builder.UseSetting("JWT:Audience", "TestAudience");
            builder.UseSetting("JWT:ExpirationMinutes", "60");
            builder.UseSetting("JWT:RefreshTokenExpirationMinutes", "1440");
            builder.UseSetting("TreasuryApiBaseUrl", "https://test.treasury.gov");

            Environment.SetEnvironmentVariable("JWT_SECRET", "test-secret-key-for-unit-tests-minimum-32-characters");
            Environment.SetEnvironmentVariable("MARIADB_HOST", "localhost");
            Environment.SetEnvironmentVariable("MARIADB_PORT", "3306");
            Environment.SetEnvironmentVariable("MARIADB_DATABASE", "testdb");
            Environment.SetEnvironmentVariable("MARIADB_USER", "testuser");
            Environment.SetEnvironmentVariable("MARIADB_PASSWORD", "testpass");

            builder.ConfigureServices(services =>
                {
                // Remove authentication/authorization for testing
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                // Replace IPurchaseService with mock
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPurchaseService));
                if (descriptor != null)
                    {
                    services.Remove(descriptor);
                    }

                services.AddScoped(_ => _mockPurchaseService.Object);
                });
            });
        }

    [Fact]
    public async Task Get_ShouldReturn200WithPurchase_WhenPurchaseExists()
        {
        // Arrange
        var purchaseId = 1;
        var expectedPurchase = new PurchaseDto
            {
            Id = purchaseId,
            Description = "Test Purchase",
            PurchaseAmount = 100.50m,
            TransactionDatetimeUtc = DateTime.UtcNow,
            TransactionIdentifier = Guid.NewGuid()
            };

        _mockPurchaseService.Setup(s => s.GetByIdAsync(purchaseId))
            .ReturnsAsync(expectedPurchase);

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/purchases/{purchaseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PurchaseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedPurchase.Id);
        result.Description.Should().Be(expectedPurchase.Description);
        result.PurchaseAmount.Should().Be(expectedPurchase.PurchaseAmount);
        result.TransactionIdentifier.Should().Be(expectedPurchase.TransactionIdentifier);

        _mockPurchaseService.Verify(s => s.GetByIdAsync(purchaseId), Times.Once);
        }
    }

// Fake policy evaluator to bypass authentication in tests
public class FakePolicyEvaluator : IPolicyEvaluator
    {
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal();
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(
            claimsPrincipal,
            "FakeScheme");
        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
        }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
        {
        var result = PolicyAuthorizationResult.Success();
        return Task.FromResult(result);
        }
    }
