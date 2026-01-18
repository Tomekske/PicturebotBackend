using Api.Core.Entities;
using Api.Core.Interfaces;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class PictureRepository(ApplicationDbContext context) : IPictureRepository
{
    public async Task CreateAsync(Picture picture)
    {
        context.Pictures.Add(picture);
        await context.SaveChangesAsync();
    }

    public async Task<List<Picture>> FindAllAsync()
    {
        return await context.Pictures.ToListAsync();
    }

    public async Task<Picture?> FindByIdAsync(int id)
    {
        return await context.Pictures
            .Include(p => p.SubFolder)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Picture>> FindByHierarchyIdAsync(int hierarchyId)
    {
        return await context.Pictures
            .Include(p => p.SubFolder)
            .Where(p => p.SubFolder!.HierarchyId == hierarchyId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Picture picture)
    {
        context.Pictures.Update(picture);
        await context.SaveChangesAsync();
    }
}