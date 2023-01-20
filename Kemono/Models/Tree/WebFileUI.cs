using System.Collections.ObjectModel;
using Kemono.Contracts.Models;
using Kemono.Core.Models.JsonModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models.Tree;

public sealed class WebFileUI : TreeLeaf
{
    public WebFileUI(WebFile file, bool haveRpc)
    {
        UseRpc.IsEnabled = haveRpc;
        var button = new SplitButton();
        Content = new StackPanel
        {
            Children =
            {
                button,
                new TextBlock{Text = $"{file.Name}", VerticalAlignment = VerticalAlignment.Center}
            }
        };
    }

    public override object Content { get; set; }
}