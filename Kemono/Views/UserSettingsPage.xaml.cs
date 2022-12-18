using Kemono.Helpers;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Kemono.Views;

public sealed partial class UserSettingsPage : Page
{
    public UserSettingsPage()
    {
        InitializeComponent();
    }

    public BuildViewModel ViewModel
    {
        get;
        private set;
    } = null!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not IServiceScope scope)
        {
            throw new ArgumentException();
        }

        ViewModel = scope.GetService<BuildViewModel>();

        base.OnNavigatedTo(e);
    }
}