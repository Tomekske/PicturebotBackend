using Api.Core.Entities;

namespace Api.Application.DTOs;

public class CreateNodeRequest
{
    public int ParentId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public List<SubFolder> SubFolders { get; set; } = [];
    public string? SourcePath { get; set; } 
}