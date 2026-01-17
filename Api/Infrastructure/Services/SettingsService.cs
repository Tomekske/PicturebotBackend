using Api.Core.Entities;
using Api.Infrastructure.Repositories;


namespace Api.Infrastructure.Services;

public class SettingsService
{
    private readonly SettingsRepository _repo;

    public SettingsService(SettingsRepository repo)
    {
        _repo = repo;
    }

    public async Task<Settings> GetSettingsAsync() => await _repo.GetSettingsAsync();

    public async Task UpdateSettingsAsync(Settings settings) => await _repo.UpdateSettingsAsync(settings);
}