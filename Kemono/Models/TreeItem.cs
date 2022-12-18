using System.Reflection;
using Windows.UI;
using Kemono.Core.Models.JsonModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Kemono.Models;

public sealed class TreeItem
{
    private static readonly SolidColorBrush R = new(new Color {A = 255, R = 255});
    private static readonly SolidColorBrush B = new(new Color {A = 255, B = 255});
    private static readonly SolidColorBrush G = new(new Color {A = 255, G = 255});
    private readonly SplitButton _button;
    private readonly CheckBox _download;
    private readonly bool _isNode;
    private readonly TextBlock _text;
    private readonly CheckBox _useRpc;
    public readonly List<TreeItem> Children = new();

    public readonly FrameworkElement Content;
    private TreeItem? _parent;

    public TreeItem(WebFile file, bool rpcEnable)
    {
        _isNode = false;

        _useRpc = new CheckBox
            {IsEnabled = rpcEnable, Content = rpcEnable ? "使用RPC" : "未配置RPC", IsChecked = file.UseRpc};
        _download = new CheckBox {Content = "下载", IsChecked = file.Download};
        _text = new TextBlock {Text = file.UseRpc ? "RPC" : "App", Foreground = file.UseRpc ? G : B};

        file.PropertyChanged += (_, _) =>
        {
            _useRpc.IsChecked = file.UseRpc;
            _download.IsChecked = file.Download;
            _text.Text = file.Download ? file.UseRpc ? "RPC" : "App" : "不下载";
            _text.Foreground = file.Download ? file.UseRpc ? G : B : R;
        };

        _button = new SplitButton
        {
            Content = _text,
            Flyout = new Flyout
            {
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition {Height = GridLength.Auto},
                        new RowDefinition {Height = GridLength.Auto}
                    },
                    Children = {_download, _useRpc}
                }
            }
        };

        _useRpc.Unchecked += (_, _) =>
        {
            _text.Text = "App";
            _text.Foreground = B;
            _parent?.FreshNodes();
            file.UseRpc = false;
        };
        _useRpc.Checked += (_, _) =>
        {
            if (!file.Download)
            {
                throw new Exception();
            }

            _text.Text = "RPC";
            _text.Foreground = G;
            _parent?.FreshNodes();
            file.UseRpc = true;
        };
        _download.Unchecked += (_, _) =>
        {
            _useRpc.IsEnabled = file.Download = false;
            _text.Text = "不下载";
            _text.Foreground = R;
            _parent?.FreshNodes();
        };
        _download.Checked += (_, _) =>
        {
            _useRpc.IsEnabled = file.Download = true;
            _text.Text = file.UseRpc ? "RPC" : "App";
            _text.Foreground = file.UseRpc ? G : B;
            _parent?.FreshNodes();
        };

        Grid.SetRow(_download, 0);
        Grid.SetRow(_useRpc, 1);

        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                _button,
                new TextBlock {Text = file.Name, VerticalAlignment = VerticalAlignment.Center}
            }
        };
    }

    private TreeItem(string text, bool rpcEnable)
    {
        _isNode = true;

        _useRpc = new CheckBox {IsEnabled = rpcEnable, Content = rpcEnable ? "全部使用RPC" : "未配置RPC", IsThreeState = true};
        _download = new CheckBox {Content = "全部下载", IsThreeState = true};
        _text = new TextBlock();

        _useRpc.Checked += (_, _) => Children.ForEach(item => item._useRpc.IsChecked = true);
        _useRpc.Unchecked += (_, _) => Children.ForEach(item => item._useRpc.IsChecked = false);
        _useRpc.Indeterminate += (_, _) =>
        {
            if (Children.All(item => item._useRpc.IsChecked == true))
            {
                _useRpc.IsChecked = false;
            }
        };
        _download.Checked += (_, _) =>
        {
            _useRpc.IsEnabled = rpcEnable;
            Children.ForEach(item => item._download.IsChecked = true);
        };
        _download.Unchecked += (_, _) =>
        {
            _useRpc.IsEnabled = false;
            Children.ForEach(item => item._download.IsChecked = false);
        };
        _download.Indeterminate += (_, _) =>
        {
            if (Children.All(item => item._download.IsChecked == true))
            {
                _download.IsChecked = false;
            }
        };

        _button = new SplitButton
        {
            Content = _text,
            Flyout = new Flyout
            {
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition {Height = GridLength.Auto},
                        new RowDefinition {Height = GridLength.Auto}
                    },
                    Children = {_download, _useRpc}
                }
            }
        };
        Grid.SetRow(_download, 0);
        Grid.SetRow(_useRpc, 1);
        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = {new TextBlock {Text = text, VerticalAlignment = VerticalAlignment.Center}}
        };
    }

    private bool NoChild => _isNode && (Children.Count == 0 || Children.All(item => item.NoChild));

    public static TreeItem FromArtist(Artist artist, bool rpcEnable)
    {
        var r = new TreeItem($"{artist.Name}[{artist.Id}]", rpcEnable);
        r.Add(artist.Posts.Select(post => FromPost(post, rpcEnable)));
        return r;
    }

    public static TreeItem FromPost(Post post, bool rpcEnable)
    {
        var r = new TreeItem($"{post.Title}[{post.Id}]", rpcEnable);
        r.Add(post.Files.Select(file => new TreeItem(file, rpcEnable)));
        return r;
    }

    public void Add(params TreeItem[] item)
    {
        if (!_isNode)
        {
            throw new TargetException();
        }

        if (Children.Count == 0)
        {
            ((StackPanel)Content).Children.Insert(0, _button);
        }

        foreach (var i in item)
        {
            i._parent = this;
            Children.Add(i);
        }

        FreshNodes();
    }

    public void Add(IEnumerable<TreeItem> items)
    {
        if (!_isNode)
        {
            throw new TargetException();
        }

        if (Children.Count == 0)
        {
            ((StackPanel)Content).Children.Insert(0, _button);
        }

        foreach (var item in items)
        {
            item._parent = this;
            Children.Add(item);
        }

        FreshNodes();
    }

    private void FreshNodes()
    {
        if (!_isNode)
        {
            throw new TargetException();
        }

        _button.Visibility = Children.Any(item => item._button.Visibility == Visibility.Visible)
            ? Visibility.Visible
            : Visibility.Collapsed;
        _useRpc.IsEnabled = Children.Any(item => item._useRpc.IsEnabled);
        _useRpc.IsChecked = Children.All(item => item.NoChild || item._useRpc.IsChecked == true)
            ? true
            : Children.All(item => item.NoChild || item._useRpc.IsChecked == false)
                ? false
                : null;
        _download.IsChecked = Children.All(item => item.NoChild || item._download.IsChecked == true)
            ? true
            : Children.All(item => item.NoChild || item._download.IsChecked == false)
                ? false
                : null;
        _text.Text = _download.IsChecked switch {true => "下载", false => "不下载", null => "部分下载"}
                     + (_download.IsChecked != false
                         ? "|" + _useRpc.IsChecked switch {true => "使用RPC", false => "使用App", null => "混合使用"}
                         : "");
        _parent?.FreshNodes();
    }
}