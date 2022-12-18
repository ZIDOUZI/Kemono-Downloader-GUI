using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Windows.ApplicationModel;
using Windows.UI;
using Kemono.Contracts.Services;
using Kemono.Helpers;
using Kemono.Models;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Kemono.ViewModels;

public class SettingsViewModel : InjectScopeViewModel
{
    private const double Error = 0.005;
    private static readonly ILocalSettingsService Settings = App.GetService<ILocalSettingsService>();
    private AcrylicSystemBackdrop _acrylic = new();
    private int _backdrop;
    private Brush _background;
    private bool _clearSucceedInfos;

    private SystemBackdrop? _current;
    private string _dateFormat = null!;
    private Visibility _debug = Visibility.Collapsed;
    private string _defaultPath = null!;
    private bool _downloadDirectly;
    private ElementTheme _elementTheme;
    private bool _enableBackdropInPages;
    private bool _exportUrlsInContent;
    private string _host = null!;

    private MicaSystemBackdrop _mica = new();
    private MicaSystemBackdrop _micaAlt = new() {Kind = MicaKind.BaseAlt};
    private string _pattern = null!;
    private string _proxy = null!;
    private int _proxyMethod;
    private bool _rememberAccount;
    private bool _showNotification;
    private IThemeSelectorService _themeSelectorService;
    private string _token = null!;
    private bool _useProxyInAria2;
    private bool _useRpc;

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = themeSelectorService.Theme;

        OnProxyMethodChanged = i => SetPropertyAndSave(ref _proxyMethod, i, nameof(ProxyMethod));

        SwitchTheme = async param =>
        {
            await themeSelectorService.SetThemeAsync(param);
        };

        OnBackdropChanged = type =>
        {
            if (!SetProperty(ref _backdrop, (int)type, nameof(Backdrop)))
            {
                return;
            }

            Settings.SaveSettingAsync(nameof(Backdrop), (int)type);
            Current = FromType(type);
            if (BackdropSettingsEnabled ^ (type != BackdropType.Defualt))
            {
                OnPropertyChanged(nameof(BackdropSettingsEnabled));
            }
        };
    }

    public bool BackdropSettingsEnabled => Backdrop != 0;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set
        {
            if (SetProperty(ref _elementTheme, value))
            {
                SwitchTheme(value);
            }
        }
    }

    public string DefaultPath
    {
        get => _defaultPath;
        set => SetPropertyAndSave(ref _defaultPath, value);
    }

    public Brush Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    private ResourceDictionary CurrentTheme =>
        (ResourceDictionary)Application.Current.Resources.MergedDictionaries[0][ElementTheme];

    private SystemBackdrop? Current
    {
        get => _current;
        set
        {
            SetProperty(ref _current, value);
            OnPropertyChanged(nameof(DarkTintOpacity));
            OnPropertyChanged(nameof(DarkTintColor));
            OnPropertyChanged(nameof(DarkLuminosityOpacity));
            OnPropertyChanged(nameof(DarkFallbackColor));
            OnPropertyChanged(nameof(LightTintOpacity));
            OnPropertyChanged(nameof(LightTintColor));
            OnPropertyChanged(nameof(LightLuminosityOpacity));
            OnPropertyChanged(nameof(LightFallbackColor));
            App.Manager.Backdrop = value;
            if (value == null)
            {
                Background = (Brush)CurrentTheme["ApplicationPageBackgroundThemeBrush"];
            }
        }
    }

    public Color DarkTintColor
    {
        get => Current?.DarkTintColor ?? Colors.Transparent;
        set
        {
            if (DarkTintColor == value || Current == null)
            {
                return;
            }

            Current.DarkTintColor = value;
            OnPropertyChanged();
        }
    }

    public double DarkTintOpacity
    {
        get => Current?.DarkTintOpacity ?? 0;
        set
        {
            if (Math.Abs(DarkTintOpacity - value) < Error || Current == null)
            {
                return;
            }

            Current.DarkTintOpacity = value;
            OnPropertyChanged();
        }
    }

    public double DarkLuminosityOpacity
    {
        get => Current?.DarkLuminosityOpacity ?? 0;
        set
        {
            if (Math.Abs(DarkLuminosityOpacity - value) < Error || Current == null)
            {
                return;
            }

            Current.DarkLuminosityOpacity = value;
            OnPropertyChanged();
        }
    }

    public Color DarkFallbackColor
    {
        get => Current?.DarkFallbackColor ?? Colors.Transparent;
        set
        {
            if (DarkFallbackColor == value || Current == null)
            {
                return;
            }

            Current.DarkFallbackColor = value;
            OnPropertyChanged();
        }
    }

    public Color LightTintColor
    {
        get => Current?.LightTintColor ?? Colors.Transparent;
        set
        {
            if (LightTintColor == value || Current == null)
            {
                return;
            }

            Current.LightTintColor = value;
            OnPropertyChanged();
        }
    }

    public double LightTintOpacity
    {
        get => Current?.LightTintOpacity ?? 0;
        set
        {
            if (Math.Abs(LightTintOpacity - value) < Error || Current == null)
            {
                return;
            }

            Current.LightTintOpacity = value;
            OnPropertyChanged();
        }
    }

    public double LightLuminosityOpacity
    {
        get => Current?.LightLuminosityOpacity ?? 0;
        set
        {
            if (Math.Abs(LightLuminosityOpacity - value) < Error || Current == null)
            {
                return;
            }

            Current.LightLuminosityOpacity = value;
            OnPropertyChanged();
        }
    }

    public Color LightFallbackColor
    {
        get => Current?.LightFallbackColor ?? Colors.Transparent;
        set
        {
            if (LightFallbackColor == value || Current == null)
            {
                return;
            }

            Current.LightFallbackColor = value;
            OnPropertyChanged();
        }
    }

    public string Host
    {
        get => _host;
        set => SetPropertyAndSave(ref _host, value);
    }

    public string Username
    {
        get;
        private set;
    } = null!;

    public string Password
    {
        get;
        private set;
    } = null!;

    public string Token
    {
        get => _token;
        set => SetPropertyAndSave(ref _token, value);
    }

    public string Proxy
    {
        get => _proxy;
        set => SetPropertyAndSave(ref _proxy, value);
    }

    public int ProxyMethod
    {
        get => _proxyMethod;
        set => OnProxyMethodChanged(value);
    }

    public bool UseRpc
    {
        get => _useRpc;
        set => SetPropertyAndSave(ref _useRpc, value);
    }

    public bool UseProxyInAria2
    {
        get => _useProxyInAria2;
        set => SetPropertyAndSave(ref _useProxyInAria2, value);
    }

    public bool ShowNotification
    {
        get => _showNotification;
        set => SetPropertyAndSave(ref _showNotification, value);
    }

    public string Pattern
    {
        get => _pattern;
        set => SetPropertyAndSave(ref _pattern, value);
    }

    public bool RememberAccount
    {
        get => _rememberAccount;
        set => SetPropertyAndSave(ref _rememberAccount, value);
    }

    public bool DownloadDirectly
    {
        get => _downloadDirectly;
        set => SetPropertyAndSave(ref _downloadDirectly, value);
    }

    public bool ClearSucceedInfos
    {
        get => _clearSucceedInfos;
        set => SetPropertyAndSave(ref _clearSucceedInfos, value);
    }

    public Visibility Debug
    {
        get => _debug;
        set => SetProperty(ref _debug, value);
    }

    public int Backdrop
    {
        get => _backdrop;
        set => OnBackdropChanged((BackdropType)value);
    }

    public string DateFormat
    {
        get => _dateFormat;
        set => SetPropertyAndSave(ref _dateFormat, value);
    }

    public bool ExportUrlsInContent
    {
        get => _exportUrlsInContent;
        set => SetPropertyAndSave(ref _exportUrlsInContent, value);
    }

    public bool EnableBackdropInPages
    {
        get => _enableBackdropInPages;
        set => SetPropertyAndSave(ref _enableBackdropInPages, value);
    }

    public event Func<ElementTheme, Task> SwitchTheme;

    public event Action<int> OnProxyMethodChanged;

    public event Action<BackdropType> OnBackdropChanged;

    //主线程死锁, https://zhuanlan.zhihu.com/p/371362645
    internal async Task Initialize()
    {
        _defaultPath = await Settings.ReadSettingAsync(nameof(DefaultPath), "");
        _proxyMethod = await Settings.ReadSettingAsync(nameof(ProxyMethod), 1);
        _proxy = await Settings.ReadSettingAsync(nameof(Proxy), "");
        _token = await Settings.ReadSettingAsync(nameof(Token), "");
        _host = await Settings.ReadSettingAsync(nameof(Host), "");
        _useRpc = await Settings.ReadSettingAsync(nameof(UseRpc), false);
        _useProxyInAria2 = await Settings.ReadSettingAsync(nameof(UseProxyInAria2), false);
        _showNotification = await Settings.ReadSettingAsync(nameof(ShowNotification), false);
        var pattern = await Settings.ReadSettingAsync<string>(nameof(Pattern));
        _pattern = string.IsNullOrEmpty(pattern)
            ? @"\{service}\{artist}[{artist_id}]\[{date}][{post_id}]{title}\{auto_named}"
            : pattern;
        _dateFormat = await Settings.ReadSettingAsync(nameof(DateFormat), "yyyy-MM-dd HH-mm");
        _clearSucceedInfos = await Settings.ReadSettingAsync(nameof(ClearSucceedInfos), false);
        _rememberAccount = await Settings.ReadSettingAsync(nameof(RememberAccount), false);
        Backdrop = await Settings.ReadSettingAsync(nameof(Backdrop), 1);
        _current = FromType((BackdropType)Backdrop);
        _enableBackdropInPages = await Settings.ReadSettingAsync(nameof(EnableBackdropInPages), false);
        _exportUrlsInContent = await Settings.ReadSettingAsync(nameof(ExportUrlsInContent), false);
        Username = await Settings.ReadSettingAsync<string>(nameof(Username), "");
        Password = await Settings.ReadSettingAsync<string>(nameof(Password), "");
        Console.WriteLine($"Version: {RuntimeHelper.Version}");
        Console.WriteLine("Current Settings:");
        Console.WriteLine($"{nameof(DefaultPath)}={DefaultPath}");
        Console.WriteLine($"{nameof(ProxyMethod)}={ProxyMethod}");
        Console.WriteLine($"{nameof(Proxy)}={Proxy}");
        Console.WriteLine($"{nameof(Token)}={Token}");
        Console.WriteLine($"{nameof(Host)}={Host}");
        Console.WriteLine($"{nameof(UseRpc)}={UseRpc}");
        Console.WriteLine($"{nameof(UseProxyInAria2)}={UseProxyInAria2}");
        Console.WriteLine($"{nameof(ShowNotification)}={ShowNotification}");
        Console.WriteLine($"{nameof(Pattern)}={Pattern}");
        Console.WriteLine($"{nameof(DateFormat)}={DateFormat}");
        Console.WriteLine($"{nameof(ClearSucceedInfos)}={ClearSucceedInfos}");
        Console.WriteLine($"{nameof(RememberAccount)}={RememberAccount}");
        Console.WriteLine($"{nameof(Backdrop)}={(BackdropType)Backdrop}");
        Console.WriteLine($"{nameof(EnableBackdropInPages)}={EnableBackdropInPages}");
    }

    private void SetPropertyAndSave<T>([NotNullIfNotNull("newValue")] ref T field, T newValue,
        [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, newValue, propertyName))
        {
            Settings.SaveSettingAsync(propertyName ?? nameof(field), newValue);
        }
    }

    internal void LoadSettings(JsonNode node)
    {
        node.TryGet<string>(nameof(DefaultPath), value => DefaultPath = value);
        node.TryGet<bool>(nameof(UseRpc), value => UseRpc = value);
        node.TryGet<string>(nameof(Host), value => Host = value);
        node.TryGet<string>(nameof(Token), value => Token = value);
        node.TryGet<int>(nameof(ProxyMethod), value => ProxyMethod = value);
        node.TryGet<string>(nameof(Pattern), value => Pattern = value);
        node.TryGet<bool>(nameof(UseProxyInAria2), value => UseProxyInAria2 = value);
        node.TryGet<bool>(nameof(ShowNotification), value => ShowNotification = value);
    }

    internal JsonNode ExportSettings()
    {
        var node = JsonNode.Parse("{}")!;
        node[nameof(DefaultPath)] = JsonValue.Create(DefaultPath);
        node[nameof(UseRpc)] = JsonValue.Create(UseRpc);
        node[nameof(Host)] = JsonValue.Create(Host);
        node[nameof(Token)] = JsonValue.Create(Token);
        node[nameof(ProxyMethod)] = JsonValue.Create(ProxyMethod);
        node[nameof(Pattern)] = JsonValue.Create(Pattern);
        node[nameof(UseProxyInAria2)] = JsonValue.Create(UseProxyInAria2);
        node[nameof(ShowNotification)] = JsonValue.Create(ShowNotification);
        return node;
    }

    public void ResetBackdrop() =>
        Current = (BackdropType)Backdrop switch
        {
            BackdropType.Defualt => null,
            BackdropType.Mica => _mica = new MicaSystemBackdrop(),
            BackdropType.MicaAlt => _micaAlt = new MicaSystemBackdrop {Kind = MicaKind.BaseAlt},
            BackdropType.Acrylic => _acrylic = new AcrylicSystemBackdrop(),
            _ => throw new ArgumentOutOfRangeException()
        };

    private SystemBackdrop? FromType(BackdropType type) => type switch
    {
        BackdropType.Defualt => null,
        BackdropType.Mica => _mica,
        BackdropType.MicaAlt => _micaAlt,
        BackdropType.Acrylic => _acrylic,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}