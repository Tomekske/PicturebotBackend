using Api.Core.Entities;
using Api.Core.Enums;
using Api.Core.Interfaces;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class HierarchyRepository(ApplicationDbContext context) : IHierarchyRepository
{
    public async Task CreateAsync(Hierarchy node)
    {
        context.Hierarchies.Add(node);
        await context.SaveChangesAsync();
    }

    public async Task<List<Hierarchy>> FindAllAsync()
    {
        return await context.Hierarchies
            .AsNoTracking()
            .Include(h => h.SubFolders)
            .ThenInclude(sf => sf.Pictures)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<bool> FindDuplicateAsync(int? parentId, string name, HierarchyType type)
    {
        var query = context.Hierarchies.AsQueryable();

        query = parentId == null
            ? query.Where(h => h.ParentId == null)
            : query.Where(h => h.ParentId == parentId);

        return await query.AnyAsync(h => h.Name == name && h.Type == type);
    }
}