using Api.Application.Interfaces;
using Api.Core.Entities;
using Api.Core.Interfaces;

namespace Api.Infrastructure.Services;

public class SettingsService(ISettingsRepository repo) : ISettingsService
{
    public async Task<Settings> GetSettingsAsync() => await repo.GetSettingsAsync();

    public async Task UpdateSettingsAsync(Settings settings) => await repo.UpdateSettingsAsync(settings);
}