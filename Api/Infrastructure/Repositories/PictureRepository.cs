using Api.Core.Entities;
using Api.Core.Interfaces;
using Api.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class PictureRepository : IPictureRepository
{
    private readonly ApplicationDBContext _context;

    public PictureRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Picture picture)
    {
        _context.Pictures.Add(picture);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Picture>> FindAllAsync()
    {
        return await _context.Pictures.ToListAsync();
    }

    public async Task<Picture?> FindByIdAsync(int id)
    {
        return await _context.Pictures
            .Include(p => p.SubFolder)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Picture>> FindByHierarchyIdAsync(int hierarchyId)
    {
        return await _context.Pictures
            .Include(p => p.SubFolder)
            .Where(p => p.SubFolder!.HierarchyId == hierarchyId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Picture picture)
    {
        _context.Pictures.Update(picture);
        await _context.SaveChangesAsync();
    }
}