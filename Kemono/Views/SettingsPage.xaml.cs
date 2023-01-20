using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aria2NET;
using Kemono.Core.Helpers;
using Kemono.Core.Models;
using Kemono.Helpers;
using Kemono.Models;
using Kemono.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Utils = Kemono.Helpers.Utils;

namespace Kemono.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        ProxyText.IsEnabled = ViewModel.ProxyMethod == 2;
        ViewModel.OnProxyMethodChanged += ProxyChanged;

        Theme.SelectedIndex = (int)ViewModel.ElementTheme;
        Backdrop.ItemsSource = Enum.GetValues<BackdropType>().Select(it => it.ToString());
        Backdrop.SelectedIndex = ViewModel.Backdrop;

        // Background = (Brush)Application.Current.Resources.MergedDictionaries[0]["ApplicationPageBackgroundThemeBrush"];
        // var background = Background;
        // ViewModel.OnBackdropChanged += type => Background = type == BackdropType.Defualt
        //     ? background
        //     : null;
    }

    public SettingsViewModel ViewModel
    {
        get;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) => ProxyChanged(ViewModel.ProxyMethod);

    private void ProxyChanged(int index)
    {
        if (index == -1)
        {
            return;
        }

        DefaultProxy.Trailer = index switch
        {
            0 => "DefaultProxyDisabledText".GetLocalized(),
            1 => "DefaultProxySystemDefaultText".GetLocalized(),
            2 => ViewModel.Proxy,
            _ => throw new ArgumentOutOfRangeException(nameof(ViewModel.ProxyMethod), index, null)
        };

        ProxyText.IsEnabled = index == 2;
    }

    private async void ChangeTheme(object sender, SelectionChangedEventArgs e)
    {
        var selected = (ComboBoxItem)((ComboBox)sender).SelectedItem;
        ViewModel.ElementTheme = Enum.Parse<ElementTheme>((string)selected.Tag);
    }

    private async void ChooseFolder(object sender, RoutedEventArgs e) =>
        ViewModel.DefaultPath = (await Utils.ChooseFolder())?.Path ?? "";


    private async void CheckRpc(object sender, RoutedEventArgs e)
    {
        try
        {
            await new Aria2NetClient(ViewModel.Host, ViewModel.Token).GetGlobalOptionAsync();
            await Utils.ShowDialog("成功", "RPC可正常使用");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            await Utils.ShowDialog("错误", "RPC检测出错");
        }
    }

    private async void ImportSettings(object sender, RoutedEventArgs e)
    {
        var file = await Utils.OpenSingleFile(".json");
        if (file == null)
        {
            return;
        }

        try
        {
            var json = await JsonSerializer.DeserializeAsync<JsonNode>(await file.OpenStreamForReadAsync());
            ViewModel.LoadSettings(json!);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Debugger.Break();
        }
    }

    private async void ExportSettings(object sender, RoutedEventArgs e)
    {
        var folder = await Utils.ChooseFolder();
        if (folder == null)
        {
            return;
        }

        try
        {
            var file = await folder.CreateFileAsync("settings.json");
            await using var stream = await file.OpenStreamForWriteAsync();
            stream.Write(JsonSerializer.SerializeToUtf8Bytes(ViewModel.ExportSettings()));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Debugger.Break();
        }
    }

    private void OpenDataFolder(object sender, RoutedEventArgs e) =>
        Process.Start("Explorer", PathHelper.AppDataPath);

    private void ShowDebug(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.Debug = Visibility.Visible;
        AppDataPath.Text = PathHelper.AppDataPath;
    }

    private void WriteConsole(object sender, RoutedEventArgs e) => Console.WriteLine("Test");

    private async void ExportLatestLog(object sender, RoutedEventArgs e) =>
        await App.CH.Export((await Utils.ChooseFolder())?.Path);

    private void ResetBackdrop(object sender, RoutedEventArgs e) => ViewModel.ResetBackdrop();
}