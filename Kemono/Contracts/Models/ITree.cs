using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Kemono.Core.Models;
using Kemono.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Kemono.Contracts.Models;

public interface ITree
{
    static RowDefinition Definition { get; } = new() {Height = GridLength.Auto};

    static SolidColorBrush Gray = new(Colors.Gray);

    static SolidColorBrush Black = new(Colors.Black);

    object Content { get; set; }

    public event Action<bool> DownloadChanged;

    public event Action<bool> UseRpcChanged;

    CheckBox Download { get; }

    CheckBox UseRpc { get; }
}

public abstract class TreeLeaf : ITree
{
    public abstract object Content { get; set; }

    public event Action<bool> DownloadChanged = delegate { };

    public event Action<bool> UseRpcChanged = delegate { };

    public CheckBox Download { get; } = new() {IsThreeState = true, Content = "Download".GetLocalized()};
    public CheckBox UseRpc { get; } = new() {IsThreeState = true, Content = "UseRpc".GetLocalized()};
}

public abstract class TreeInternal : ITree
{
    public TreeInternal(IEnumerable<ITree> nodes)
    {
        foreach (var child in nodes)
        {
            Children.Add(child);
            // 节点状态改变, 同步改变所有子节点状态
            DownloadChanged += b => child.Download.IsChecked = b;
            UseRpcChanged += b => child.UseRpc.IsChecked = b;

            // 子节点状态改变, 通知节点检查自身状态
            child.Download.Click += (_, _) => UpdateNodeDownload(child);
            child.UseRpc.Click += (_, _) => UpdateNodeUseRpc(child);
        }

        // RPC是否可用
        UseRpc.IsEnabled = Children.Any(child => child.UseRpc.IsEnabled);

        Download.Click += (sender, _) =>
        {
            var cb = (CheckBox) sender;
            var s = cb.IsChecked switch
            {
                true => false,
                false => true,
                _ => true
            };

            // 改变checkbox的状态, 检查节点组状态, 更新其他节点状态
            DownloadChanged(s);
        };

        UseRpc.Click += (sender, _) =>
        {
            var cb = (CheckBox) sender;
            var s = cb.IsChecked switch
            {
                true => false,
                false => true,
                _ => true
            };

            // 改变checkbox的状态, 检查节点组状态, 更新其他节点状态
            UseRpcChanged(s);
        };
    }

    public object Content { get; set; } = null!;

    private void UpdateNodeDownload(ITree initiator)
    {
        var r = initiator.Download.IsChecked;

        if (Children.Any(child => child.Download.IsChecked != r && child != initiator) && Download.IsChecked != null)
            Download.IsChecked = null;

        if (r != Download.IsChecked)
            Download.IsChecked = r;
        else
            Debugger.Break();
    }

    private void UpdateNodeUseRpc(ITree initiator)
    {
        var r = initiator.UseRpc.IsChecked;

        if (Children.Any(child => child.UseRpc.IsChecked != r && child != initiator) && UseRpc.IsChecked != null)
            UseRpc.IsChecked = null;

        if (r != UseRpc.IsChecked)
            UseRpc.IsChecked = r;
        else
            Debugger.Break();
    }

    public event Action<bool> DownloadChanged = delegate {};

    public event Action<bool> UseRpcChanged = delegate {};

    public CheckBox Download { get; } = new() {IsThreeState = false, Content = "Download".GetLocalized()};
    public CheckBox UseRpc { get; } = new() {IsThreeState = false, Content = "UseRpc".GetLocalized()};

    public ObservableCollection<ITree> Children { get; } = new();
}