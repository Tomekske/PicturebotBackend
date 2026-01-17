using Api.Core.Entities;

namespace Api.Application.Interfaces;

public interface IPictureService
{
    Task CreatePictureAsync(Picture picture);
    Task<List<Picture>> GetPicturesAsync();
    Task<Picture?> FindByIdAsync(int id);
    Task<List<Picture>> FindByHierarchyIdAsync(int id);
    Task UpdatePictureAsync(Picture picture);
    Task<List<List<Picture>>> GroupSimilarPicturesAsync(int hierarchyId, int threshold);
}