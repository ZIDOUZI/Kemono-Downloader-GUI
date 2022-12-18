using System.Text.Json.Nodes;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using Kemono.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinRT;
using WinRT.Interop;

namespace Kemono.Helpers;

internal static class Utils
{
    public static readonly ICommand ChooseFolderCommand = new AsyncRelayCommand(ChooseFolder);

    public static void TryGet<T>(this JsonNode node, string propertyName, ref T setValue)
    {
        try
        {
            setValue = node[propertyName]!.GetValue<T>();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static void TryGet<T>(this JsonNode node, string propertyName, Action<T> setValue)
    {
        try
        {
            setValue(node[propertyName]!.GetValue<T>());
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static async Task<StorageFolder?> ChooseFolder()
    {
        var picker = new FolderPicker();
        InitializeWithWindow.Initialize(picker, App.HWND);
        picker.FileTypeFilter.Add("*");
        return await picker.PickSingleFolderAsync();
    }

    public static async Task<StorageFile?> OpenSingleFile(params string[] type)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.HWND);
        if (type.Length != 0)
        {
            foreach (var s in type)
            {
                picker.FileTypeFilter.Add(s);
            }
        }
        else
        {
            picker.FileTypeFilter.Add("*");
        }

        return await picker.PickSingleFileAsync();
    }

    public static async Task<IReadOnlyList<StorageFile>> OpenMultipleFile(params string[] type)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, App.HWND);
        if (type.Length != 0)
        {
            // TODO: 检测可用性
        }

        foreach (var s in type)
        {
            picker.FileTypeFilter.Add(s);
        }

        return await picker.PickMultipleFilesAsync();
    }

    /*public static async Task<StorageFolder> ChooseFile()
    {
        var filePicker = new FolderPicker();
        InitializeWithWindow.Initialize(filePicker, WindowNative.GetWindowHandle(App.MainWindow));
        filePicker.FileTypeFilter.Add("*");
        return await filePicker.PickSingleFolderAsync();
    }*/


    public static T GetService<T>(this IServiceScope scope)
        where T : class
    {
        if (scope.ServiceProvider.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        if (typeof(T).IsAssignableTo(typeof(InjectScopeViewModel)))
        {
            service.As<InjectScopeViewModel>().SetScope(scope);
        }

        return service;
    }

    public static IAsyncOperation<ContentDialogResult> ShowDialog(
        string title = "",
        string content = "",
        string primary = "确认",
        string? secondary = null, string? close = null) => new ContentDialog
    {
        XamlRoot = App.GlobalRoot,
        Title = title,
        Content = content,
        PrimaryButtonText = primary,
        SecondaryButtonText = secondary,
        CloseButtonText = close
    }.ShowAsync();

    public static IAsyncOperation<ContentDialogResult> ShowDialog(
        string title,
        object content,
        string primary = "确认",
        string? secondary = null, string? close = null) =>
        new ContentDialog
        {
            XamlRoot = App.GlobalRoot,
            Title = title,
            Content = content,
            PrimaryButtonText = primary,
            SecondaryButtonText = secondary,
            CloseButtonText = close
        }.ShowAsync();
}