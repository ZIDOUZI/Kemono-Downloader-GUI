using System.Collections.ObjectModel;
using Kemono.Models;

namespace Kemono.ViewModels;

public class MainViewModel : InjectScopeViewModel
{
    public ObservableCollection<TabItemViewModel> Vms
    {
        get;
    } = new();
}