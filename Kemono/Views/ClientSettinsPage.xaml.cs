using Kemono.Core.Models;
using Kemono.Helpers;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Kemono.Views;

public sealed partial class ClientSettingsPage : Page
{
    private readonly Domain[] _domains = Enum.GetValues<Domain>();

    private readonly Dictionary<UserAgent, string> _userAgents = new()
    {
        {
            UserAgent.Edge,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.134 Safari/537.36 Edg/103.0.1264.77"
        },
        {
            UserAgent.Chrome,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36"
        },
        {UserAgent.BaiduNetdisk, "netdisk;6.0.0.12;PC;PC-Windows;10.0.16299;WindowsBaiduYunGuanJia"},
        {UserAgent.Transmission, "Transmission/2.94"},
        {UserAgent.Aria2, "aria2/1.35.0"}
    };

    public ClientSettingsPage()
    {
        InitializeComponent();
    }

    public BuildViewModel ViewModel
    {
        get;
        private set;
    } = null!;

    private Resolver.Builder Builder => ViewModel.Builder;

    private double Timeout
    {
        get => Builder.Timeout;
        set => Builder.Timeout = (int)value;
    }

    private double Retry
    {
        get => Builder.Retry;
        set => Builder.Retry = (int)value;
    }

    private double Delay
    {
        get => Builder.Delay;
        set => Builder.Delay = (int)value;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not IServiceScope scope)
        {
            throw new ArgumentException();
        }

        ViewModel = scope.GetService<BuildViewModel>();
        if (_userAgents.ContainsValue(Builder.UserAgent))
        {
            Ua.SelectedItem = _userAgents.First(pair => pair.Value == Builder.UserAgent).Key;
        }
        else if (Enum.TryParse<UserAgent>(Builder.UserAgent, out var agent))
        {
            Ua.SelectedItem = agent;
        }
        else
        {
            Ua.Text = Builder.UserAgent;
        }

        base.OnNavigatedTo(e);
    }

    private void InputProxy(object sender, TextChangedEventArgs e)
    {
        if (Builder.Proxy == "")
        {
            return;
        }

        UsernameBlock.Visibility = Visibility.Visible;
        PasswordBlock.Visibility = Visibility.Visible;
    }

    private void TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        try
        {
            var ua = Enum.Parse<UserAgent>(args.Text);
            sender.SelectedItem = ua;
            Builder.UserAgent = _userAgents[ua];
        }
        catch (Exception)
        {
            sender.SelectedItem = args.Text;
            Builder.UserAgent = args.Text;
        }
    }
}