using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Kemono.Contracts.Services;

public interface INavigationService
{
    bool CanGoBack
    {
        get;
    }

    bool CanGoForward
    {
        get;
    }

    IServiceScope? Scope
    {
        get;
        set;
    }


    Frame? Frame
    {
        get;
        set;
    }

    event NavigatedEventHandler Navigated;

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();

    bool GoForward();
}