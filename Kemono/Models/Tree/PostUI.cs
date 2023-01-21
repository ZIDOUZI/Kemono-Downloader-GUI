using System.Collections.ObjectModel;
using Kemono.Contracts.Models;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models.Tree;

public sealed class PostUI : TreeInternal
{
    public Post Post { get; }

    public PostUI(Post post, IEnumerable<WebFileUI> files) : base(files)
    {
        Post = post;
        var download = new SymbolIcon(Symbol.Download);
        var useRpc = new SymbolIcon(Symbol.Remote);

        DownloadChanged += b => download.Foreground = b ? ITree.Black : ITree.Gray;
        UseRpcChanged += b => useRpc.Foreground = b ? ITree.Black : ITree.Gray;

        // SplitButton初始化
        var button = new SplitButton
        {
            Content = new StackPanel {Children = {download, useRpc}},
            Flyout = new Flyout
            {
                Content = new Grid
                {
                    RowDefinitions = {ITree.Definition, ITree.Definition},
                    Children = {Download, UseRpc}
                }
            },
            IsEnabled = Children.Any()
        };

        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                button,
                new TextBlock {Text = $"{post.Title}[{post.Id}]", VerticalAlignment = VerticalAlignment.Center}
            }
        };
    }
}