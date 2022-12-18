using Kemono.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.ViewModels;

public class TabItemViewModel : InjectScopeViewModel
{
    public readonly CancellationTokenSource Source = new();
    private string _text = "新建标页";

    public TabItemViewModel()
    {
        SetScopeEvent += scope => Scope = scope;
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public IServiceScope Scope
    {
        get;
        private set;
    } = null!;

    public Frame RootFrame
    {
        get;
    } = new();

    public CancellationToken Token => Source.Token;

    public void NavigateTo<T>() where T : class => RootFrame.Navigate(typeof(T), Scope);
}