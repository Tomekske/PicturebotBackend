using Api.Core.Entities;
using Api.Core.Enums;
using Api.Core.Interfaces;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class HierarchyRepository : IHierarchyRepository
{
    private readonly ApplicationDBContext _context;

    public HierarchyRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Hierarchy node)
    {
        _context.Hierarchies.Add(node);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Hierarchy>> FindAllAsync()
    {
        return await _context.Hierarchies
            .Include(h => h.SubFolders)
            .ThenInclude(sf => sf.Pictures)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<bool> FindDuplicateAsync(int? parentId, string name, HierarchyType type)
    {
        var query = _context.Hierarchies.AsQueryable();

        if (parentId == null)
            query = query.Where(h => h.ParentId == null);
        else
            query = query.Where(h => h.ParentId == parentId);

        return await query.AnyAsync(h => h.Name == name && h.Type == type);
    }
}