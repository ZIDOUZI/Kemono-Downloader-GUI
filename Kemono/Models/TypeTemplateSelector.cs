using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models;

public class TypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate StringTemplate
    {
        get;
        set;
    } = null!;

    public DataTemplate ElementTemplate
    {
        get;
        set;
    } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        // Select one of the DataTemplate objects, based on the
        // value of the selected item in the ComboBox.
        if (item is string)
        {
            return StringTemplate;
        }

        if (typeof(FrameworkElement).IsAssignableTo(item.GetType()))
        {
            return ElementTemplate;
        }

        throw new ArgumentException();
    }
}