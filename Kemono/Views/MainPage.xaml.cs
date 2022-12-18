using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;
using CommunityToolkit.Mvvm.Input;
using Kemono.Helpers;
using Kemono.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kemono.Views;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private static readonly TabItemViewModel SettingViewModel = App.CreateNewViewModel();

    private readonly string _versionDescription = GetVersionDescription();

    /*private new void DragOverEvent(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Copy;

    private async new void DropEvent(object sender, DragEventArgs e)
    {
        /*var supportTypes = new List<string> { ".yaml", ".yml" };
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var error = new List<StorageFile>();
            if (items.Count <= 0) return;
            foreach (var storageItem in items)
            {
                var file = (StorageFile)storageItem;
                if (file == null) throw new ArgumentException();
                if (!supportTypes.Contains(file.FileType)) continue;
                var reader = new StreamReader(await file.OpenStreamForReadAsync());
                var yaml = new YamlStream();
                yaml.Load(reader);
                var node = (YamlMappingNode)yaml.Documents[0].RootNode;
                if (node["Version"].ToString() != "1.0")
                {
                    error.Add(file);
                    continue;
                }

                var newTab = new TabViewItem
                {
                    IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                    Header = "New Session",
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // The Content of a TabViewItem is often a frame which hosts a page.
                var frame = new Frame { Margin = new Thickness(16) };
                newTab.Content = frame;
                frame.Navigate(typeof(TabItemPage), frame);
                ((TabItemPage)frame.Content).Parse(node);
                Root.TabItems.Add(newTab);
            }

            if (Root.SelectedIndex == -1) Root.SelectedIndex = 0;
            var text = error.Aggregate("yaml文件读取错误\n以下为错误文件:", (current, file) => current + file.Name);
            await Dialog(text);
        }
        else await Dialog("仅支持yaml文件.");#1#
    }*/

    internal readonly ICommand ShowDialog =
        new AsyncRelayCommand<ContentDialog>(async param => await param!.ShowAsync());

    public MainPage()
    {
        InitializeComponent();
        Root.TabItemsSource = Vms;
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(CustomDragRegion);

        SettingViewModel.Text = "设置页";
        SettingViewModel.RootFrame.Content = App.GetService<SettingsPage>();

        // SettingViewModel.RootFrame.Margin = (Thickness)Application.Current.Resources["NavigationViewPageContentMargin"];
        Vms.CollectionChanged += (_, _) =>
        {
            if (Vms.Count == 0)
            {
                Application.Current.Exit();
            }
        };
    }

    private static ObservableCollection<TabItemViewModel> Vms => App.MainViewModel.Vms;

    private void OnLaunched(object sender, RoutedEventArgs routedEventArgs) => AddTab(Root, 0);

    private void AddTab(TabView sender, object args)
    {
        var vm = App.CreateNewViewModel();
        vm.RootFrame.Content = new BuildPage(vm.Scope);

        Vms.Add(vm);

        Root.SelectedIndex = Vms.IndexOf(vm);
    }

    // Remove the requested tab from the TabView
    private void CloseTab(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        lock (Vms)
        {
            var vm = Vms.First(vm => vm.RootFrame == (Frame)args.Tab.Content);

            vm.Source.Cancel();
            Vms.Remove(vm);
        }
    }

    private void AddTabKeyboard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        AddTab((TabView)args.Element, args);
        args.Handled = true;
    }

    private void CloseTabKeyboard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var frame = (Frame)((TabViewItem)((TabView)args.Element).SelectedItem).Content;

        lock (Vms)
        {
            var vm = Vms.First(vm => vm.RootFrame == frame);
            vm.Scope.GetService<TabItemViewModel>().Source.Cancel();
            Vms.Remove(vm);
        }

        args.Handled = true;
    }

    private void NavigateToSettings(object sender, RoutedEventArgs e)
    {
        int index;
        if (!Vms.Contains(SettingViewModel))
        {
            index = Vms.Count;
            Vms.Add(SettingViewModel);
        }
        else
        {
            index = Vms.IndexOf(SettingViewModel);
        }

        Root.SelectedIndex = index;
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build,
                packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return
            $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}