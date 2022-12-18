using Kemono.Contracts.Services;
using Kemono.Helpers;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kemono.Views;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BuildPage : Page
{
    private readonly DownloadViewModel _download;
    private readonly TabItemViewModel _vm;

    public BuildPage(IServiceScope scope)
    {
        ViewModel = scope.GetService<BuildViewModel>();
        _vm = scope.GetService<TabItemViewModel>();
        _download = scope.GetService<DownloadViewModel>();

        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);
        ViewModel.NavigateTo(typeof(DownloadSettingsPage).FullName!);

        TitleBarHelper.UpdateTitleBar(RequestedTheme);
    }

    private BuildViewModel ViewModel
    {
        get;
    }

    private void GoBackKey(KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void GoForwardKey(KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoForward();

        args.Handled = result;
    }

    private async void BuildAndNavigate(object o, RoutedEventArgs routedEventArgs) => await Build();

    private async void BuildKeyboard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) =>
        await Build();

    private async Task Build()
    {
        var old = Content;
        var ring = new ProgressRing { Width = 50, Height = 50 };
        Content = ring;
        if (ViewModel.Login)
        {
            try
            {
                if (!await ViewModel.Builder.Login(ViewModel.Username, ViewModel.Password))
                {
                    var dialog = new ContentDialog
                    {
                        XamlRoot = XamlRoot,
                        Title = "登录失败",
                        Content = "请检查账号和密码是否有误.\n 若不需要登录, 请取消勾选\"记住用户名和密码\"",
                        PrimaryButtonText = "确认"
                    };

                    await dialog.ShowAsync();
                    Content = old;
                    return;
                }
            }
            catch (Exception e)
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "登录失败",
                    Content = e + "请检查网络连接.",
                    PrimaryButtonText = "确认"
                };

                await dialog.ShowAsync();
                Content = old;
                Console.WriteLine(e);
                return;
            }
        }

        if (ViewModel.Host != "" && ViewModel.Builder.UseRpc)
        {
            if (!await ViewModel.Builder.Aria2Config(ViewModel.Host, ViewModel.Token))
            {
                await Utils.ShowDialog("设置Aria2RPC失败", "请检查Host和Token(若设置了token)是否有误.\n 若不需要RPC, 请取消勾选\"启用RPC\"");
                Content = old;
                return;
            }
        }

        try
        {
            _download.Downloader = await ViewModel.Builder.Build();
            await _download.Downloader.DownloadArtists(
                total =>
                {
                    ring.Maximum = total;
                    ring.IsIndeterminate = false;
                },
                added => ring.Value += added
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Utils.ShowDialog("请求失败", e + "请检查网络连接");
            Content = old;
            return;
        }

        _vm.NavigateTo<DownloadPage>();
    }
}