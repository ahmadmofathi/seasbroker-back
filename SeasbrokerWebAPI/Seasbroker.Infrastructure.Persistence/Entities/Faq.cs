using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seasbroker.Infrastructure.Persistence.Entities;

[Table("faqs")]
public class Faq : AuditableEntity
{
    [Required]
    [MaxLength(500)]
    public string Heading { get; set; } = string.Empty;

    [Required]
    public string Para { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
