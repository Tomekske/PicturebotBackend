using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Api.Core.Enums;

namespace Api.Core.Entities;

[Index(nameof(Type))] 
[Index(nameof(CurationStatus))]
[Index(nameof(PHash))]
[Table("picture")]
public class Picture
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [JsonPropertyName("file_name")]
    public required string FileName { get; set; }

    [JsonPropertyName("index")]
    public string? Index { get; set; }

    [JsonPropertyName("extension")]
    public string? Extension { get; set; }
    
    [JsonPropertyName("type")]
    public PictureType Type { get; set; } 

    [JsonPropertyName("location")]
    public string? Location { get; set; }
    
    [JsonPropertyName("curation_status")]
    public CurationStatus CurationStatus { get; set; } = CurationStatus.Unflagged;

    [JsonPropertyName("sharpness")]
    public int Sharpness { get; set; }

    [JsonPropertyName("phash")]
    public ulong PHash { get; set; }
    
    // Foreign Key to SubFolder
    [Column("sub_folder_id")]
    public int SubFolderId { get; set; }

    [ForeignKey(nameof(SubFolderId))]
    public SubFolder? SubFolder { get; set; }
}
