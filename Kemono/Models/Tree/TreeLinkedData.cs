using System.ComponentModel;
using System.Runtime.CompilerServices;
using Kemono.Models.Tree.Interface;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.Models;

public abstract class TreeLinkedData : ITree
{

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void CreateLinkedData<T>(ref T source, Func<T[], T> func, [CallerMemberName] string propertyName = null)
    {
        if (HaveChildren)
        {
            FreshNodesEvent += source = func(Children.Select(tree => tree.source));
        }
    }

    public ICollection<TreeLinkedData> Children
    {
        get;
    }

    public bool HaveChildren
    {
        get;
    }

    public event Action? FreshNodesEvent;
    public void FreshNodes() => throw new NotImplementedException();
}