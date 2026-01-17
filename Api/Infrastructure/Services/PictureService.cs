using System.Numerics;
using Api.Application.Interfaces; 
using Api.Core.Entities;
using Api.Core.Interfaces; 

namespace Api.Infrastructure.Services;

public class PictureService : IPictureService
{
    private readonly IPictureRepository _repo;

    public PictureService(IPictureRepository repo)
    {
        _repo = repo;
    }

    public Task CreatePictureAsync(Picture picture) => _repo.CreateAsync(picture);

    public Task<List<Picture>> GetPicturesAsync() => _repo.FindAllAsync();

    public Task<Picture?> FindByIdAsync(int id) => _repo.FindByIdAsync(id);

    public Task<List<Picture>> FindByHierarchyIdAsync(int id) => _repo.FindByHierarchyIdAsync(id);

    public Task UpdatePictureAsync(Picture picture) => _repo.UpdateAsync(picture);

    public async Task<List<List<Picture>>> GroupSimilarPicturesAsync(int hierarchyId, int threshold)
    {
        // Now calling the repository interface method
        var pictures = await _repo.FindByHierarchyIdAsync(hierarchyId);
        var groups = new List<List<Picture>>();

        foreach (var pic in pictures)
        {
            bool foundGroup = false;

            foreach (var group in groups)
            {
                bool similarToAll = true;
                foreach (var groupPic in group)
                {
                    if (HammingDistance((ulong)pic.PHash, (ulong)groupPic.PHash) > threshold)
                    {
                        similarToAll = false;
                        break;
                    }
                }

                if (similarToAll)
                {
                    group.Add(pic);
                    foundGroup = true;
                    break;
                }
            }

            if (!foundGroup)
            {
                groups.Add(new List<Picture> { pic });
            }
        }

        return groups;
    }

    private static int HammingDistance(ulong h1, ulong h2)
    {
        return BitOperations.PopCount(h1 ^ h2);
    }
}