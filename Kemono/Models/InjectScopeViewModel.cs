using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Kemono.Models;

public class InjectScopeViewModel : ObservableRecipient
{
    public delegate void SetScopeAction(IServiceScope scope);

    public event SetScopeAction SetScopeEvent = delegate
    {
    };

    public void SetScope(IServiceScope scope) => SetScopeEvent(scope);
}