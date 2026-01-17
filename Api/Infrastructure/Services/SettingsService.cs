using Api.Application.Interfaces;
using Api.Core.Entities;

namespace Api.Infrastructure.Services;

public class SettingsService(ISettingsService repo) : ISettingsService
{
    public async Task<Settings> GetSettingsAsync() => await repo.GetSettingsAsync();

    public async Task UpdateSettingsAsync(Settings settings) => await repo.UpdateSettingsAsync(settings);
}