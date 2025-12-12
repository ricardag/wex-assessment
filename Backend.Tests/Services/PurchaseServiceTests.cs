using Backend.Application.Dtos;
using Backend.Application.Services;
using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class PurchaseServiceTests
    {
    private readonly Mock<IPurchaseRepository> _mockRepository;
    private readonly PurchaseService _sut;

    public PurchaseServiceTests()
        {
        _mockRepository = new Mock<IPurchaseRepository>();
        _sut = new PurchaseService(_mockRepository.Object);
        }

    [Fact]
    public async Task CreateAsync_ShouldCreatePurchaseWithGeneratedTransactionIdentifier()
        {
        // Arrange
        var createDto = new CreatePurchaseDto
            {
            Description = "Test Purchase",
            PurchaseAmount = 150.50m,
            TransactionDateUtc = DateTime.UtcNow
            };

        var capturedPurchase = (Purchase?)null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
            .Callback<Purchase>(p => capturedPurchase = p)
            .ReturnsAsync((Purchase p) => new Purchase
                {
                Description = p.Description,
                PurchaseAmount = p.PurchaseAmount,
                TransactionDatetimeUtc = p.TransactionDatetimeUtc,
                TransactionIdentifier = p.TransactionIdentifier
                });

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(createDto.Description);
        result.PurchaseAmount.Should().Be(createDto.PurchaseAmount);
        result.TransactionDatetimeUtc.Should().Be(createDto.TransactionDateUtc);
        result.TransactionIdentifier.Should().NotBe(Guid.Empty);

        capturedPurchase.Should().NotBeNull();
        capturedPurchase!.TransactionIdentifier.Should().NotBe(Guid.Empty);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Purchase>()), Times.Once);
        }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingPurchaseSuccessfully()
        {
        // Arrange
        var purchaseId = 1;
        var existingPurchase = new Purchase
            {
            Description = "Old Description",
            PurchaseAmount = 100.00m,
            TransactionDatetimeUtc = DateTime.UtcNow.AddDays(-1),
            TransactionIdentifier = Guid.NewGuid()
            };

        var updateDto = new UpdatePurchaseDto
            {
            Description = "Updated Description",
            PurchaseAmount = 200.00m,
            TransactionDateUtc = DateTime.UtcNow
            };

        _mockRepository.Setup(r => r.GetByIdAsync(purchaseId))
            .ReturnsAsync(existingPurchase);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Purchase>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(purchaseId, updateDto);

        // Assert
        existingPurchase.Description.Should().Be(updateDto.Description);
        existingPurchase.PurchaseAmount.Should().Be(updateDto.PurchaseAmount);
        existingPurchase.TransactionDatetimeUtc.Should().Be(updateDto.TransactionDateUtc);

        _mockRepository.Verify(r => r.GetByIdAsync(purchaseId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingPurchase), Times.Once);
        }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenPurchaseDoesNotExist()
        {
        // Arrange
        var nonExistentId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(nonExistentId))
            .ReturnsAsync((Purchase?)null);

        // Act
        var act = async () => await _sut.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Purchase with ID {nonExistentId} not found");

        _mockRepository.Verify(r => r.GetByIdAsync(nonExistentId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
