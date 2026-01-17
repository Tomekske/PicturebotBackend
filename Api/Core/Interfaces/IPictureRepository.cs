using Api.Core.Entities;

namespace Api.Core.Interfaces;

public interface IPictureRepository
{
    Task CreateAsync(Picture picture);
    Task<List<Picture>> FindAllAsync();
    Task<Picture?> FindByIdAsync(int id);
    Task<List<Picture>> FindByHierarchyIdAsync(int hierarchyId);
    Task UpdateAsync(Picture picture);
}