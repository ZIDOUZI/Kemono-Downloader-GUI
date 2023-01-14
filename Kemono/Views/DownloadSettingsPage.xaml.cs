using Kemono.Core.Models;
using Kemono.Helpers;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Utils = Kemono.Helpers.Utils;

namespace Kemono.Views;

public sealed partial class DownloadSettingsPage : Page
{
    public DownloadSettingsPage()
    {
        InitializeComponent();
    }

    public BuildViewModel Vm
    {
        get;
        private set;
    } = null!;

    private Resolver.Builder Builder => Vm.Builder;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not IServiceScope scope)
        {
            throw new ArgumentException($"{e.Parameter}");
        }

        Vm = scope.GetService<BuildViewModel>();

        base.OnNavigatedTo(e);
    }

    private async void ChooseFolder(object sender, RoutedEventArgs e)
    {
        DefaultPath.IsEnabled = false;
        Vm.DefaultPath = (await Utils.ChooseFolder())?.Path ?? Vm.DefaultPath;
        DefaultPath.IsEnabled = true;
    }
}