using Api.Core.Entities;

namespace Api.Application.Interfaces;

public interface ISettingsService
{
    Task<Settings> GetSettingsAsync();
    Task UpdateSettingsAsync(Settings settings);
}