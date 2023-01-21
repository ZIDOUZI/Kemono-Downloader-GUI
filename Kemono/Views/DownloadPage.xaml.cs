using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Downloader;
using Kemono.Contracts.Models;
using Kemono.Core.Contracts.Services;
using Kemono.Core.Helpers;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Helpers;
using Kemono.Models;
using Kemono.Models.Tree;
using Kemono.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

// ReSharper disable LocalizableElement

namespace Kemono.Views;

public sealed partial class DownloadPage : Page
{
    private static readonly Serializer Serializer = new();
    private static readonly Regex UrlRegex = new(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");

    private IDownload? _d;

    private DownloadPackage? _interrupt;
    // private IEnumerator<Artist> _e = null!;

    private bool _finished;
    private IServiceScope? _scope;
    private TabItemViewModel _vm = null!;
    private StreamWriter _writer = null!;
    private DownloadViewModel Vm = null!;
    private static readonly Thickness Zero = new(0);
    private static readonly Thickness Default = new(24, 8, 4, 8);

    private int[] _posotion = new int[3];

    private static ContentDialog ErrorDialog => new()
        {XamlRoot = App.GlobalRoot, Title = "未知错误", SecondaryButtonText = "确认"};

    private bool Downloading => Interrupt.IsEnabled && Interrupt.IsChecked == false;

    #region Constructor

    public DownloadPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_scope != null)
            return;

        if (e.Parameter is not IServiceScope scope)
            throw new ArgumentException($"{e.GetType()} should not be null", nameof(e));

        _scope = scope;
        _vm = scope.GetService<TabItemViewModel>();
        Vm = scope.GetService<DownloadViewModel>();
        _writer = App.Settings.ExportUrlsInContent
            ? new StreamWriter(Path.Combine(Vm.Resolver.DefaultPath, "export.yaml"),
                new FileStreamOptions
                    {Share = FileShare.ReadWrite, Mode = FileMode.Append, Access = FileAccess.Write})
            : StreamWriter.Null;

        Add(new TextBlock {Text = $"已加载画师数量: {Vm.Resolver.ArtistCount}"});
        if (!Vm.Resolver.LoggedIn)
            Load.IsEnabled = false;

        _vm.Text = "下载页";
        base.OnNavigatedTo(e);
    }

    #endregion

    #region Controls Action

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.Text = "正在输入";
        Solve.IsEnabled = true;
        Download.IsEnabled = false;
    }

    private void Artist_OnClick(object sender, RoutedEventArgs e) => LoadFavoriteArtists();

    private void Post_OnClick(object sender, RoutedEventArgs e) => LoadFavoritePosts();

    private async void Download_OnClick(object sender, RoutedEventArgs e) => await StartDownload();

    private void Pause_OnClick(object sender, RoutedEventArgs e) => PauseDownload();

    private async void Stop_OnClick(object sender, RoutedEventArgs e) => await StopDownload();

    private async void Solve_OnClick(object sender, RoutedEventArgs e) => await LoadItems();

    private void Filter_OnClick(object sender, RoutedEventArgs e) => _vm.RootFrame.Navigate(typeof(FilterPage), _scope);

    private void Clear_OnClick(object sender, RoutedEventArgs e) => Infos.Children.Clear();

    #endregion

    #region Generate Functions

    public async Task<ArtistUI> GetArtist(Artist artist) =>
        new(artist, from post in await Vm.Resolver.GetPosts(artist) select GetPosts(post));

    public async IAsyncEnumerable<ArtistUI> GetArtists(IEnumerable<Artist> artists)
    {
        foreach (var artist in artists) yield return await GetArtist(artist);
    }

    public PostUI GetPosts(Post post) =>
        new(post, Vm.Resolver.GetFiles(post).Select(file => new WebFileUI(file, Vm.HaveRpc)));

    public static ContentDialog UnhandledError(Exception e)
    {
        var dialog = ErrorDialog;
        if (App.CH == null)
        {
            dialog.Content = e.Message + "\n由于控制台加载异常, 相关日志未被记录.";
        }
        else
        {
            dialog.PrimaryButtonText = "打开日志文件夹";
            dialog.Content = e.Message;
            dialog.PrimaryButtonClick += (_, _) => PathHelper.OpenFolder(PathHelper.AppDataPath);
        }

        return dialog;
    }

    public void Add(params UIElement[] e)
    {
        // TODO: 滑至页面末端
        var bottom = Math.Abs(Viewer.VerticalOffset - Viewer.ScrollableHeight) < 0.05;
        foreach (var element in e)
        {
            Infos.Children.Add(element);
        }

        if (bottom) Viewer.ScrollToVerticalOffset(Viewer.ScrollableHeight);
    }

    public bool Remove(UIElement e) => Infos.Children.Remove(e);

    private bool? SetTime(DateTime? time, FileSystemInfo? file)
    {
        if (file is not {Exists: true})
            return null;

        if (time is not { } date)
            return false;

        try
        {
            file.CreationTime = date;
            file.LastWriteTime = date;
            file.Refresh();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine($"Modifier {(file is FileInfo ? "File" : "Dir")} '{file.FullName}' failed");
            Add(new TextBlock {Text = $"    {(file is FileInfo ? "  修改文件" : "修改文件夹")}时间失败"});
            Debugger.Break();
            return false;
        }
    }

    // Async 关键词问题
    // private Task ResetEnumerator() => Task.Run(() =>
    // {
    //     _e.Reset();
    //     foreach (var artist in Vm.Artists)
    //     {
    //         artist.Enumerator.Reset();
    //         foreach (var post in artist.Posts)
    //         {
    //             post.Enumerator.Reset();
    //         }
    //     }
    // });

    private static void ShowNotification(AppNotification notification) => throw new NotImplementedException();

    #endregion

    #region Predownload

    private async void LoadFavoriteArtists()
    {
        try
        {
            Vm.Resolved = new ObservableCollection<ArtistUI>(
                GetArtists(await Vm.Resolver.GetFavoriteArtists()).ToEnumerable()
            );
            Add(new TextBlock {Text = $"已加载{Vm.Resolved.Count}个画师"});
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Add(new TextBlock {Text = "加载收藏画师失败. 请检查网络连接"});
            Debugger.Break();
        }
    }

    private async void LoadFavoritePosts()
    {
        try
        {
            Vm.Resolved = new ObservableCollection<ArtistUI>(
                (await Vm.Resolver.GetFavoritePosts())
                .GroupBy(post => Vm.Resolver.Parse(post))
                .Select(g => new ArtistUI(g.Key, g.Select(p => Vm.Resolver.GetFiles(p, Vm.HaveRpc))))
            );

            Add(new TextBlock {Text = $"已加载{Vm.Resolved.Count}篇收藏贴"});
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Add(new TextBlock {Text = "加载收藏贴失败. 请检查网络连接"});
            Debugger.Break();
        }
    }

    private async Task LoadItems()
    {
        Solve.IsEnabled = Download.IsEnabled = false;
        Load.IsEnabled = false;
        Ring.IsIndeterminate = true;

        _vm.Text = "正在解析";
        Console.WriteLine("I: Start Resolve");
        Vm.Resolved = new ObservableCollection<ArtistUI>();

        foreach (var url in UrlBox.Text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            Console.WriteLine(url);
            try
            {
                // TODO: 解析pixiv, fanbox等链接
                var artist = await Vm.Resolver.Parse(new Uri(url.Trim()));
                var au = await GetArtist(artist);
                Vm.Resolved.Add(au);
                Add(new TextBlock {Text = $"成功解析画师{artist.Name}({artist.Id}, 共{au.Children.Count}篇post"});
            }
            catch (AmbiguousMatchException e)
            {
                Console.WriteLine(e);
                Add(new TextBlock {Text = $"{url}解析失败, 请检查是否输入错误"});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await UnhandledError(e).ShowAsync();
                Debugger.Break();
            }
        }

        // _e = Vm.Artists.GetEnumerator();

        Solve.IsEnabled = true;
        Download.IsEnabled = Vm.Resolved.Any();
        Load.IsEnabled = Vm.Resolver.LoggedIn;
        Ring.IsIndeterminate = false;
        _vm.Text = "解析完成";
    }

    #endregion

    #region Download

    private async Task StartDownload()
    {
        _vm.Text = "准备下载";
        Console.WriteLine("I: Start Download.");

        Solve.IsEnabled = Download.IsEnabled = false;
        Load.IsEnabled = false;
        _finished = Ring.IsIndeterminate = Interrupt.IsEnabled = true;

        var node = new YamlMappingNode();

        foreach (var a in Vm.Resolved)
        {
            var artist = a.Artist;
            if (a.Download.IsChecked == false)
            {
                Console.WriteLine($"I: 已跳过画师{artist.Name}: 未勾选.");
                break;
            }

            if (!Downloading) break;

            Add(new TextBlock {Text = $"├当前画师: {artist.Name}[{artist.Service}-{artist.Id}]", Tag = true});
            Console.WriteLine($"I: Current artist: {artist.Name}");

            var map = new YamlMappingNode();

            await foreach (var (title, sequence) in DealArtist(artist))
            {
                if (!Downloading)
                {
                    break;
                }

                map.Add(title, sequence);
            }

            if (map.Any()) node.Add(artist.Name, map);
        }

        if (node.Any())
        {
            await _writer.WriteLineAsync("---");
            await _writer.WriteLineAsync($"# {DateTime.Now}");
            Serializer.Serialize(_writer, node);
            await _writer.WriteLineAsync("...");
            var b = new HyperlinkButton {Content = "保存url成功. 点击打开目录", Padding = Zero};
            b.Click += (_, _) => Process.Start("explorer", Vm.Resolver.DefaultPath);
            Add(b);
        }

        if (App.Settings.ClearSucceedInfos)
        {
            Infos.Children.DropWhere(e => e is FrameworkElement {Tag: true});
        }

        if (_finished)
        {
            // _e.Reset();
            // await Task.Run(() => Vm.Artists.ForEach(artist =>
            // {
            //     artist.Enumerator.Reset();
            //     artist.Posts.ForEach(post => post.Enumerator.Reset());
            // }));
            _vm.Text = "下载完成";
            Add(new TextBlock {Text = "下载已完成"});
        }
        else
        {
            return;
        }

        Solve.IsEnabled = Download.IsEnabled = true;
        Load.IsEnabled = Vm.Resolver.LoggedIn;
        Ring.IsIndeterminate = Interrupt.IsEnabled = false;
        foreach (var obj in Vm.Resolved)
        {
        }

        _vm.Text = "下载完成";
        Add(new TextBlock {Text = "下载已完成"});
    }

    private async IAsyncEnumerable<(string title, YamlSequenceNode sequence)> DealArtist(Artist artist)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task<(string title, YamlSequenceNode? sequence)> ForPost(Post post) =>
            (post.Title, await DealPost(post));

        await foreach (var (title, sequence) in artist.Enumerator.DoWork(ForPost))
        {
            if (!Downloading)
                break;

            if (sequence != null)
                yield return (title, sequence);
        }
    }

    private async Task<YamlSequenceNode?> DealPost(Post post)
    {
        // TODO: 不下载时不创建文件夹
        // TODO: Post的Content可选下载
        if (!post.Files.Any())
        {
            Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock {Text = $"│├{post.Title} 无文件, "},
                    new HyperlinkButton {Content = "导航到对应网址", NavigateUri = post.Link, Padding = Zero}
                }
            });
            return null;
        }

        Add(new TextBlock {Text = $"│├当前文章: {post.Title}[{post.Id}]", Tag = true});
        // TODO: 显示日期
        Console.WriteLine($"I: Current post: {post.Id}");
        var date = post.Published?.DateTime;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task<bool?> ForFile(WebFile file) => SetTime(date, await DealWebFile(file));

        await foreach (var success in post.Enumerator.DoWork(ForFile))
        {
            if (success != null)
                _d = null;

            if (!Downloading)
                break;
        }

        var dir = Path.GetDirectoryName(post.Files.First().File);
        SetTime(date, new DirectoryInfo(dir!));

        if (post.SaveContent)
        {
            _vm.Text = "正在保存内容";
            try
            {
                Add(new TextBlock
                    {
                        Text = (Vm.Resolver.DownloadContent(post)
                            ? "││└成功保存关联内容: "
                            : "││└无关联内容: ") + $"{post.Id}({post.Title})",
                        Tag = true
                    }
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Add(new TextBlock {Text = "││└保存关联内容失败: " + $"{post.Id}({post.Title})"});
                Debugger.Break();
            }
        }

        // TODO: 解压文件

        if (!App.Settings.ExportUrlsInContent)
            return null;

        var node = new YamlSequenceNode();
        foreach (Match match in UrlRegex.Matches(post.Content))
        {
            node.Add(match.Value);
        }

        return node;
    }

    private async Task<FileInfo?> DealWebFile(WebFile file)
    {
        if (!file.Download)
            return null;

        if (file.UseRpc)
        {
            await PostToRpc(file);
            return null;
        }

        if (_interrupt != null)
        {
            Console.WriteLine("I: Continue to download file" + file.Name);

            _d = DownloadBuilder.Build(_interrupt);
        }
        else
        {
            // 捕获构造函数异常: 文件打开错误等
            try
            {
                _d = Vm.Resolver.Download(file);

                var name = Path.GetFileName(file.File);

                if (_d == null)
                {
                    Add(new TextBlock {Text = $"││├文件 {name} 已存在.", Tag = true});
                    Console.WriteLine($"I: {Path.GetFileName(file.File)} is exist.");
                    return null;
                }

                _vm.Text = "准备下载";
                var tb = new TextBlock {Text = $"││├准备下载文件 {name}.", Tag = file.Url};
                var pb = new ProgressBar {Margin = Default};
                Add(tb, pb);

                _d.DownloadProgressChanged += (_, args) => pb.Value = args.TotalBytesToReceive;
                _d.DownloadStarted += (_, args) =>
                {
                    pb.Maximum = args.TotalBytesToReceive;
                    _vm.Text = "正在下载";
                    tb.Text = $"││├正在下载文件: {name}";
                };
                _d.DownloadFileCompleted += (_, args) =>
                {
                    if (args.Cancelled)
                    {
                        _vm.Text = "用户打断";
                        tb.Text = $"││└已打断下载文件 {name}.";
                    }
                    else if (args.Error != null)
                    {
                        _vm.Text = "出现错误";
                        tb.Text = $"││├文件 {name} 下载失败.";
                        // TODO: 提示用户出错
                        Console.Error.WriteLine(args.Error.Message);
                    }
                    else
                    {
                        _vm.Text = "下载成功";
                        tb.Text = $"││├文件 {name} 已下载.";
                        tb.Tag = true;
                    }
                };
                await _d.StartAsync();

                return new FileInfo(_d.Filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await UnhandledError(e).ShowAsync();
                return null;
            }
        }
    }

    private async Task<bool> PostToRpc(WebFile info)
    {
        var node = JsonSerializer.Deserialize<JsonNode>(await Vm.Resolver.Post(info, App.Settings.UseProxyInAria2));
        var success = node!["error"] == null;
        var text = success
            ? $"││├{info.Name} 已推送至aria2. 返回id:{node["id"]}"
            : $"││├{info.Name} 推送失败! 错误详情:\n   {node["error"]}";
        Add(new TextBlock {Text = text, Tag = node["error"] == null});
        return success;
    }

    private async void PauseDownload()
    {
        if (Interrupt.IsChecked == true)
        {
            _d?.Pause();
            _vm.Text = "已暂停";
            _finished = false;
            Ring.IsIndeterminate = false;
        }
        else
        {
            await StartDownload();
        }
    }

    private async Task StopDownload()
    {
        _d?.Stop();
        _vm.Text = "已停止";
        _interrupt = _d?.Package;
        Add(new TextBlock {Text = "已停止下载"});
        Solve.IsEnabled = Download.IsEnabled = true;
        Load.IsEnabled = Vm.Resolver.LoggedIn;
        _finished = Ring.IsIndeterminate = Interrupt.IsEnabled = false;
        await ResetEnumerator();
    }

    #endregion
}