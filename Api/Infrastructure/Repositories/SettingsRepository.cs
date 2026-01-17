using Api.Core.Entities;
using Api.Core.Interfaces;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly ApplicationDBContext _context;

    public SettingsRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<Settings> GetSettingsAsync()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            settings = new Settings { Id = 1 };
            _context.Settings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(Settings settings)
    {
        // Re-fetch to ensure we are updating the tracked entity
        var existing = await GetSettingsAsync();
        
        existing.ThemeMode = settings.ThemeMode;
        existing.LibraryPath = settings.LibraryPath;
        
        await _context.SaveChangesAsync();
    }
}