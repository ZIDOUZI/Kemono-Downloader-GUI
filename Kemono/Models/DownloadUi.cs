using System.Diagnostics;
using System.Net.NetworkInformation;
using Kemono.Core.Contracts.Services;
using Kemono.ViewModels;
using Kemono.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// ReSharper disable LocalizableElement

namespace Kemono.Models;

public sealed class DownloadUi : IDownload<FileInfo>
{
    private static readonly Thickness Margin = new(24, 8, 4, 8);
    private readonly IDownload<FileInfo> _download;
    private const string Message = "似乎加载了很久了, 这已经超出了正常的请求时间.\n请检查网络连接, 点击确认继续加载, 点击取消停止下载";
    public readonly ContentDialog TooLongTimeDialog = new()
    {
        XamlRoot = App.GlobalRoot,
        Title = "提示",
        Content = Message,
        PrimaryButtonText = "确认",
        SecondaryButtonText = "取消"
    };

    public DownloadUi(IDownload<FileInfo> d, TabItemViewModel vm, DownloadPage page, string name)
    {
        var text = new TextBlock {Text = "││├准备下载" };
        var bar = new ProgressBar {IsIndeterminate = true, Margin = Margin};
        page.Add(text);
        page.Add(bar);
        _download = d;
        _download.ProgressChanged += added => bar.Value += added;
        _download.LengthCallback += l => bar.Maximum = l;
        _download.StateChanged += state =>
        {
            switch (state)
            {
                case DownloadState.Created:
                case DownloadState.Initializing:
                    vm.Text = "准备下载";
                    break;
                case DownloadState.Initialized:
                    vm.Text = "开始下载";
                    break;
                case DownloadState.Downloading:
                    vm.Text = "正在下载";
                    text.Text = $"││├正在下载文件: {name}";
                    bar.ShowPaused = bar.IsIndeterminate = false;
                    break;
                case DownloadState.Faulted:
                    vm.Text = "下载失败";
                    text.Text = $"││├下载文件失败: {name}";
                    bar.ShowError = true;
                    break;
                case DownloadState.Paused:
                    vm.Text = "已暂停";
                    text.Text = $"││└文件 {name} 已暂停下载.";
                    bar.ShowPaused = true;
                    Console.WriteLine($"I: {name} is paused to download.");
                    break;
                case DownloadState.Canceled:
                    vm.Text = "已停止";
                    text.Text = $"││└文件 {name} 已停止下载.";
                    bar.ShowError = true;
                    Console.WriteLine($"I: {name} is stopped to download.");
                    break;
                case DownloadState.Succeed:
                    text.Text = $"││├文件 {name} 已下载.";
                    text.Tag = true;
                    page.Remove(bar);
                    Console.WriteLine($"I: {name} download successful.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        };
    }

    public DownloadState State => _download.State;

    public bool Running => _download.Running;

    public long Length => _download.Length;

    public Task Init() => throw new NotImplementedException();

    public async Task<FileInfo?> Start()
    {
        if (_download.State == DownloadState.Created)
        {
            var timer = new DispatcherTimer();
            timer.Tick += async (_, _) =>
            {
                if (TooLongTimeDialog.Visibility != Visibility.Visible)
                {
                    await TooLongTimeDialog.ShowAsync();
                }
            };
            timer.Interval = TimeSpan.FromSeconds(12);
            timer.Start();
            var r = await Initialize();
            timer.Stop();
            if (!r || State != DownloadState.Initialized)
            {
                return null;
            }
        }

        return await _download.Start();
    }

    public void Dispose() => _download.Dispose();
    public ValueTask DisposeAsync() => _download.DisposeAsync();
    public void Pause() => _download.Pause();
    public void Stop() => _download.Stop();
    public event Action<int> ProgressChanged = _ => throw new NotSupportedException();
    public event Action<DownloadState> StateChanged = _ => throw new NotSupportedException();
    public event Action<long> LengthCallback = _ => throw new NotSupportedException();

    private async Task<bool> Initialize()
    {
        do
        {
            try
            {
                await _download.Init();
                return true;
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("W: Received an unexpected EOF or 0 bytes from the transport stream.");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("W: Cannot send the same request message multiple times.");
                await Task.Delay(500);
                Debugger.Break();
            }
            catch (NetworkInformationException)
            {
                Console.WriteLine("E: Unhandled Network Exception.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await DownloadPage.UnhandledError(e).ShowAsync();
            }
        } while (_download.Running);

        return false;
    }
}