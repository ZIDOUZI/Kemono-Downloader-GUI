using Kemono.Contracts.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models.Tree;

public class Selector : DataTemplateSelector
{
    public DataTemplate NodeTemplate { get; set; }
    public DataTemplate ItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return item is TreeInternal ? NodeTemplate : ItemTemplate;
    }
}