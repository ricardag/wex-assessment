using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Domain.Entities;

[Table("Currency")]
public class CountryCurrency
    {
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required]
    [Column("country")]
    [MaxLength(256)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Column("currency")]
    [MaxLength(256)]
    public string Currency { get; set; } = string.Empty;
    }