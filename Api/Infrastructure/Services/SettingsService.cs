using Api.Application.Interfaces;
using Api.Core.Entities;

namespace Api.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsService _repo;

    public SettingsService(ISettingsService repo)
    {
        _repo = repo;
    }

    public async Task<Settings> GetSettingsAsync() => await _repo.GetSettingsAsync();

    public async Task UpdateSettingsAsync(Settings settings) => await _repo.UpdateSettingsAsync(settings);
}