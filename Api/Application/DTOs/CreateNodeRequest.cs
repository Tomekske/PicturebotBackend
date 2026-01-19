using System.Text.Json.Serialization;
using Api.Core.Entities;

namespace Api.Application.DTOs;

public class CreateNodeRequest
{
    [JsonPropertyName("parent_id")]
    public int ParentId { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    [JsonPropertyName("sub_folders")]
    public List<SubFolder> SubFolders { get; set; } = [];
    
    [JsonPropertyName("source_path")]
    public string? SourcePath { get; set; }
}