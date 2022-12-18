using System.ComponentModel;
using Windows.UI.Xaml.Markup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kemono.Models;

[DefaultProperty(nameof(Content))]
[ContentProperty(Name = nameof(Content))]
public sealed class SettingExpander : Control
{
    public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty.Register(
        nameof(PrimaryText), typeof(string), typeof(SettingExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty.Register(
        nameof(SecondaryText), typeof(string), typeof(SettingExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content), typeof(FrameworkElement), typeof(SettingExpander), new PropertyMetadata(null));

    public static readonly DependencyProperty TrailerProperty = DependencyProperty.Register(
        nameof(Trailer), typeof(object), typeof(SettingExpander), new PropertyMetadata(default));

    public static readonly DependencyProperty TrailerVisibilityProperty = DependencyProperty.Register(
        nameof(TrailerVisibility), typeof(int), typeof(SettingExpander), new PropertyMetadata(default));

    public SettingExpander()
    {
        DefaultStyleKey = typeof(SettingExpander);
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

    public FrameworkElement Content
    {
        get => (FrameworkElement)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object Trailer
    {
        get => GetValue(TrailerProperty);
        set => SetValue(TrailerProperty, value);
    }

    public Visibility TrailerVisibility
    {
        get => (Visibility)GetValue(TrailerVisibilityProperty);
        set => SetValue(TrailerVisibilityProperty, (int)value);
    }
}