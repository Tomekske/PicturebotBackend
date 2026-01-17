using Api.Core.Entities;

namespace Api.Application.Services;

public interface ISettingsService
{
    Task<Settings> GetSettingsAsync();
    Task UpdateSettingsAsync(Settings settings);
}