using Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class SettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Settings> GetSettingsAsync()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            // Fallback if seeding didn't run or database was cleared
            settings = new Settings { Id = 1 };
            _context.Settings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(Settings settings)
    {
        var existing = await GetSettingsAsync();
        existing.ThemeMode = settings.ThemeMode;
        existing.LibraryPath = settings.LibraryPath;
        await _context.SaveChangesAsync();
    }
}