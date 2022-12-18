using Kemono.Contracts.Services;
using Kemono.Views;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<DownloadSettingsPage>();
        Configure<UserSettingsPage>();
        Configure<ClientSettingsPage>();
    }

    public Type GetPageType(string key)
    {
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out var pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }

            return pageType;
        }
    }

    private void Configure<V>()
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(V).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException(
                    $"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}