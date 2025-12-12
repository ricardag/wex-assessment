using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Dtos;

public class UpdatePurchaseDto
    {
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(50, ErrorMessage = "Description must not exceed 50 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Purchase amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Purchase amount must be greater than zero")]
    public decimal PurchaseAmount { get; set; }
     
    [Required(ErrorMessage = "Transaction Date is required")]
    public DateTime TransactionDateUtc { get; set; }
    }
