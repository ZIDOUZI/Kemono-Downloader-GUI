using Microsoft.UI.Xaml;

namespace Kemono.Contracts.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme
    {
        get;
    }

    event Action<ElementTheme> OnThemeChanged;

    Task InitializeAsync();

    Task SetThemeAsync(ElementTheme theme);

    Task SetRequestedThemeAsync();
}