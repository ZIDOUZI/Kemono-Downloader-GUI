namespace Kemono.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task<T> ReadSettingAsync<T>(string key, T fallback);

    Task SaveSettingAsync<T>(string key, T value);
}