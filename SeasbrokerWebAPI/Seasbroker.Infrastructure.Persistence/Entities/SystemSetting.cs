using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seasbroker.Infrastructure.Persistence.Entities;

[Table("system_settings")]
public class SystemSetting : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}
