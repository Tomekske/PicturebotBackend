using Api.Core.Entities;

namespace Api.Core.Interfaces;

public interface ISettingsRepository
{
    Task<Settings> GetSettingsAsync();
    Task UpdateSettingsAsync(Settings settings);
}