using Api.Core.Entities;
using Api.Core.Enums;

namespace Api.Core.Interfaces;

public interface IHierarchyRepository
{
    Task CreateAsync(Hierarchy node);
    Task<List<Hierarchy>> FindAllAsync();
    Task<bool> FindDuplicateAsync(int? parentId, string name, HierarchyType type);
}