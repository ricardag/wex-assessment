using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Domain.Entities;

[Table("Purchases")]
public class Purchase
    {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required]
    [Column("description")]
    [MaxLength(50)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("transaction_datetime_utc")]
    public DateTime TransactionDatetimeUtc { get; set; }

    [Required]
    [Column("purchase_amount", TypeName = "decimal(10,2)")]
    public decimal PurchaseAmount { get; set; }

    [Required]
    [Column("transaction_identifier")]
    [MaxLength(36)]
    public Guid TransactionIdentifier { get; init; }
    }
