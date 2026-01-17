using Api.Data.Models;
using Api.Data.Repositories;

namespace Api.Services;

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