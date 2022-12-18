using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kemono.Models;

public class NotifyPropertyValue<TValue> : INotifyPropertyChanged
{
    private TValue _value;

    public NotifyPropertyValue(TValue value)
    {
        _value = value;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            if (value.Equals(_value))
            {
                return;
            }

            _value = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}