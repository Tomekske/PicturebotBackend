using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entities;

[Table("settings")]
public class Settings
{
    [Key]
    public int Id { get; set; }

    [Column("theme_mode")]
    public string ThemeMode { get; set; } = "system";

    [Column("library_path")]
    public string LibraryPath { get; set; } = string.Empty;
}