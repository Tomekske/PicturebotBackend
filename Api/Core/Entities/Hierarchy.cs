using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Data.Enums;

namespace Api.Data.Models;

[Table("hierarchies")]
public class Hierarchy
{
    [Key]
    public int Id { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    // Navigation property for the recursive relationship
    [ForeignKey(nameof(ParentId))]
    public Hierarchy? Parent { get; set; }

    [Required]
    [Column("type")]
    public HierarchyType Type { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    // Storing UUID as string to match char(36)
    [MaxLength(36)]
    [Column("uuid")]
    public string? Uuid { get; set; }

    // Navigation properties
    public ICollection<Hierarchy> Children { get; set; } = new List<Hierarchy>();
    
    public ICollection<SubFolder> SubFolders { get; set; } = new List<SubFolder>();
}