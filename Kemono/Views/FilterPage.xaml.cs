using Kemono.Core.Models;
using Kemono.Helpers;
using Kemono.Models;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Kemono.Views;

public sealed partial class FilterPage : Page
{
    private Frame _frame = null!;
    private IEnumerable<TreeItem> _items = null!;
    public DownloadViewModel ViewModel = null!;

    public FilterPage()
    {
        InitializeComponent();
    }

    private long Before => BeforeDate.SelectedDate?.Date.Date.Add(BeforeTime.Time).ToUnixTimeMilliseconds() ??
                           long.MaxValue;

    private long After => AfterDate.SelectedDate?.Date.Date.Add(AfterTime.Time).ToUnixTimeMilliseconds() ?? 0;


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not IServiceScope scope)
            throw new ArgumentException(nameof(e.Parameter));

        _frame = scope.GetService<TabItemViewModel>().RootFrame;
        ViewModel = scope.GetService<DownloadViewModel>();

        _items = ViewModel.Artists.Select(artist => TreeItem.FromArtist(artist, ViewModel.Resolver.HaveRpc));
    }

    private void ImageSelectionChanged(object sender, RoutedEventArgs e) =>
        ViewModel.Artists.ForEach(artist => artist.Posts.ForEach(post => post.Files.ForEach(info =>
        {
            if (info.File.EndsWith(".zip"))
            {
                return;
            }

            switch (((Button)sender).Tag)
            {
                case "0":
                    info.Download = false;
                    break;
                case "1":
                    info.Download = info.UseRpc = true;
                    break;
                case "2":
                    info.Download = true;
                    info.UseRpc = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        })));

    private void FileSelectionChanged(object sender, RoutedEventArgs e) =>
        ViewModel.Artists.ForEach(artist => artist.Posts.ForEach(post => post.Files.ForEach(info =>
        {
            if (!info.File.EndsWith(".zip"))
            {
                return;
            }

            switch (((Button)sender).Tag)
            {
                case "0":
                    info.Download = false;
                    break;
                case "1":
                    info.Download = info.UseRpc = true;
                    break;
                case "2":
                    info.Download = true;
                    info.UseRpc = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        })));

    private void GoBack(object sender, RoutedEventArgs e) => _frame.GoBack();

    private void FreshDate(DatePicker sender, DatePickerSelectedValueChangedEventArgs args) => SetLimit();
    private void FreshTime(TimePicker sender, TimePickerSelectedValueChangedEventArgs args) => SetLimit();

    private void SetLimit()
    {
        if (Before <= After)
        {
            AfterDate.SelectedDate = null;
            AfterTime.SelectedTime = null;
        }

        foreach (var artist in ViewModel.Artists)
        {
            foreach (var post in artist.Posts)
            {
                if (After <= post.TimeStamp && post.TimeStamp <= Before || post.TimeStamp == -1)
                {
                    post.Files.ForEach(file => file.Download = true);
                }
                else
                {
                    post.Files.ForEach(file => file.Download = false);
                }
            }
        }
    }

    private void SelectUndownloaded(object sender, RoutedEventArgs e)
    {
        // TODO
        /*foreach (var (artist, posts) in ViewModel.Dict)
        {
            var yaml = Utils.Yaml($"/{artist.service}/{artist.id}");
            foreach (var (post, infos) in posts)
            {
                if (yaml[0].RootNode.TryGet(""))
                {
                    infos.ForEach((_, info, i) => info.Download = true);
                }
                else
                {
                    infos.ForEach((_, info) => info.Download = true);
                }
            }
        }*/
        // throw new NotImplementedException();
    }
}