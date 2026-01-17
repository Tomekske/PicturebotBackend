using Api.Core.Entities;
using Api.Core.Interfaces;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repositories;

public class SettingsRepository(ApplicationDbContext context) : ISettingsRepository
{
    public async Task<Settings> GetSettingsAsync()
    {
        var settings = await context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        if (settings == null)
        {
            settings = new Settings { Id = 1 };
            context.Settings.Add(settings);
            await context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task UpdateSettingsAsync(Settings settings)
    {
        // Re-fetch to ensure we are updating the tracked entity
        var existing = await GetSettingsAsync();

        existing.ThemeMode = settings.ThemeMode;
        existing.LibraryPath = settings.LibraryPath;

        await context.SaveChangesAsync();
    }
}