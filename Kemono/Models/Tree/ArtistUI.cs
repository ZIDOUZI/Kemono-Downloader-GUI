using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Kemono.Contracts.Models;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models.Tree;

public sealed class ArtistUI : TreeInternal
{
    public ArtistUI(Artist artist, IEnumerable<PostUI> posts) : base(posts)
    {
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
                new TextBlock {Text = $"{artist.Name}[{artist.Id}]", VerticalAlignment = VerticalAlignment.Center}
            }
        };
    }

}