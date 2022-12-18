using Kemono.Core.Models.JsonModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models;

public class TreeViewTemplateSelector : DataTemplateSelector
{
    public DataTemplate ArtistTemplate
    {
        get;
        set;
    } = null!;

    public DataTemplate PostTemplate
    {
        get;
        set;
    } = null!;

    public DataTemplate FileTemplate
    {
        get;
        set;
    } = null!;

    protected override DataTemplate SelectTemplateCore(object item) =>
        item switch
        {
            // Select one of the DataTemplate objects, based on the
            // value of the selected item in the ComboBox.
            Artist => ArtistTemplate,
            Post => PostTemplate,
            WebFile => FileTemplate,
            _ => throw new ArgumentException()
        };
}