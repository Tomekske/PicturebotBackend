using Api.Data.Models;
using Api.Data.Enums;

namespace Api.Repositories;

public class HierarchyRepository
{
    private readonly ApplicationDbContext _context;

    public HierarchyRepository(ApplicationDbContext context)
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