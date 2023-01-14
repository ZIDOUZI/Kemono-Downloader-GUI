using System.Diagnostics;
using System.Text;
using Kemono.Activation;
using Kemono.Contracts.Services;
using Kemono.Core.Contracts.Services;
using Kemono.Core.Helpers;
using Kemono.Core.Services;
using Kemono.Helpers;
using Kemono.Models;
using Kemono.Services;
using Kemono.ViewModels;
using Kemono.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace Kemono;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    public static readonly ConsoleHelper? CH = ConsoleHelper.CreateInstance(PathHelper.AppDataPath,
        DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".log");

    public static readonly SettingsViewModel Settings = GetService<SettingsViewModel>();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

                // Services
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddScoped<INavigationService, NavigationService>();
                services.AddScoped<INavigationViewService, NavigationViewService>();

                // Core Services
                services.AddSingleton<IFileService, FileService>();

                // Views and ViewModels
                services.AddScoped<TabItemViewModel>();

                services.AddScoped<BuildViewModel>();
                services.AddScoped<BuildPage>();

                services.AddScoped<ClientSettingsPage>();
                services.AddScoped<UserSettingsPage>();
                services.AddScoped<DownloadSettingsPage>();

                services.AddScoped<DownloadViewModel>();
                services.AddScoped<FilterPage>();
                services.AddScoped<DownloadPage>();

                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<SettingsPage>();

                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();

                // Configuration
                services.Configure<LocalSettingsOptions>(
                    context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            }).Build();

        var notification = GetService<IAppNotificationService>();
        notification.Initialize();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => notification.Unregister();

        UnhandledException += App_UnhandledException;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static IntPtr HWND { get; } = MainWindow.GetWindowHandle();


    public static WindowManager Manager { get; } = WindowManager.Get(MainWindow);

    public static MainViewModel MainViewModel { get; } = new();

    public static XamlRoot GlobalRoot => MainWindow.Content.XamlRoot;

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static IServiceScope CreateScope() => (Current as App)!.Host.Services.CreateScope();

    public static TabItemViewModel CreateNewViewModel() => CreateScope().GetService<TabItemViewModel>();

    private static void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        Console.WriteLine(e);
        CH?.Dispose();

        var sb = new StringBuilder()
            .AppendLine("Unhandled Exception.")
            .AppendLine($"Message: {e.Message}")
            .AppendLine($"Data: {e.Exception.Data}")
            .AppendLine("===ExceptionInfo===")
            .AppendLine($"ClassName: {e.Exception.GetType()}")
            .AppendLine($"Message: {e.Exception.Message}")
            .AppendLine($"Data: {e.Exception.Data}")
            .AppendLine($"InnerException: {e.Exception.InnerException}")
            .AppendLine($"HelpURL: {e.Exception.HelpLink}")
            .AppendLine($"Source: {e.Exception.Source}")
            .AppendLine($"HResult: {e.Exception.HResult:x8}")
            .AppendLine("StackTrace:")
            .AppendLine(e.Exception.StackTrace);

        PathHelper.SaveText("kemono-error.log", sb.ToString());

        Process.Start("Explorer", PathHelper.AppDataPath);

        e.Handled = true;
        Current.Exit();
        // var toast = new ToastContentBuilder()
        //     .AddArgument("conversationId", 0x01)
        //     .AddText("致命错误.")
        //     .AddText("详见 文档/kemono-error.log 文件.")
        //     .AddButton(new ToastButton()
        //         .SetContent("打开文件夹")
        //         .AddArgument("action")
        //         .SetBackgroundActivation());
        //
        // GetService<IAppNotificationService>().Show(toast.GetXml().GetXml());
        // TODO: NOTIFICATION
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await Settings.Initialize();

        await GetService<IActivationService>().ActivateAsync(args);
    }
}