using System.ComponentModel;
using Windows.UI.Xaml.Markup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kemono.Models;

[DefaultProperty(nameof(Trailer))]
[ContentProperty(Name = nameof(Trailer))]
public sealed class SettingItem : Control
{
    public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty.Register(
        nameof(PrimaryText), typeof(string), typeof(SettingItem), new PropertyMetadata(null));

    public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty.Register(
        nameof(SecondaryText), typeof(string), typeof(SettingItem), new PropertyMetadata(null));

    public static readonly DependencyProperty TrailerProperty = DependencyProperty.Register(
        nameof(Trailer), typeof(FrameworkElement), typeof(SettingItem), new PropertyMetadata(null));

    public SettingItem()
    {
        DefaultStyleKey = typeof(SettingItem);
    }

    public string PrimaryText
    {
        get => (string)GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    public FrameworkElement Trailer
    {
        get => (FrameworkElement)GetValue(TrailerProperty);
        set
        {
            value.HorizontalAlignment = HorizontalAlignment.Right;
            value.VerticalAlignment = VerticalAlignment.Center;
            SetValue(TrailerProperty, value);
            Grid.SetColumn(value, 1);
        }
    }
}