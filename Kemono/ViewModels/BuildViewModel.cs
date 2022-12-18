using System.Net;
using System.Runtime.CompilerServices;
using Kemono.Contracts.Services;
using Kemono.Core.Models;
using Kemono.Helpers;
using Kemono.Models;
using Kemono.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;

namespace Kemono.ViewModels;

public class BuildViewModel : InjectScopeViewModel
{
    private static readonly ILocalSettingsService Settings = App.GetService<ILocalSettingsService>();

    public readonly Downloader.Builder Builder;
    private bool _isBackEnabled;

    private bool _login = App.Settings.RememberAccount;
    private string _password = App.Settings.Password;

    private IServiceScope _scope = null!;
    private object? _selected;
    private string _username = App.Settings.Username;

    public string Host = App.Settings.Host;
    public string Token = App.Settings.Token;
    public bool UseProxyInAria2 = App.Settings.UseProxyInAria2;

    public BuildViewModel()
    {
        SetScopeEvent += scope =>
        {
            _scope = scope;
            NavigationViewService = scope.GetService<INavigationViewService>();
            NavigationViewService.Scope = scope;
            NavigationService = scope.GetService<INavigationService>();
            NavigationService.Scope = scope;
            NavigationService.Navigated += OnNavigated;
        };
        Builder = new Downloader.Builder
        {
            Proxy = App.Settings.ProxyMethod switch
            {
                0 => "",
                1 => WebRequest.DefaultWebProxy?.GetProxy(new Uri("https://kemono.party"))?.ToString() ?? "",
                2 => App.Settings.Proxy,
                _ => throw new IndexOutOfRangeException()
            },
            DefaultPath = App.Settings.DefaultPath,
            UseRpc = App.Settings.UseRpc,
            Pattern = App.Settings.Pattern,
            DateFormat = App.Settings.DateFormat
        };
    }

    public bool Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Username
    {
        get => _username;
        set => SetPropertyAndSave(ref _username, value, App.Settings.RememberAccount);
    }

    public string Password
    {
        get => _password;
        set => SetPropertyAndSave(ref _password, value, App.Settings.RememberAccount);
    }

    public INavigationService NavigationService
    {
        get;
        private set;
    } = null!;

    public INavigationViewService NavigationViewService
    {
        get;
        private set;
    } = null!;

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        private set => SetProperty(ref _isBackEnabled, value);
    }

    public bool UseRpc
    {
        get => Builder.UseRpc;
        set => SetProperty(ref Builder.UseRpc, value);
    }

    public bool Overwrite
    {
        get => Builder.Overwrite;
        set => SetProperty(ref Builder.Overwrite, value);
    }

    public string DefaultPath
    {
        get => Builder.DefaultPath;
        set => SetProperty(ref Builder.DefaultPath, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public bool NavigateTo(string pageKey) => NavigationService.NavigateTo(pageKey, _scope);

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    private void SetPropertyAndSave<T>(ref T value, T newValue, bool save = false,
        [CallerMemberName] string name = null!)
    {
        SetProperty(ref value, newValue, name);
        if (save)
        {
            Settings.SaveSettingAsync(name, newValue);
        }
    }
}