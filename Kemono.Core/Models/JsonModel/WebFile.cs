using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using static System.IO.Path;

namespace Kemono.Core.Models.JsonModel;

public class WebFile : INotifyPropertyChanged
{
    private bool _download = true;
    private bool _useRpc;

    public string File;

    public string Url;
    // TODO: 替换notnull, 测试可用性

    [JsonPropertyName("id")]
    public object Id
    {
        get;
        set;
    } = null;
    // https://kemono.party/patreon/user/7537478?o=450 $19.attachment
    // https://kemono.party/fanbox/user/39128535?o=50 $13.attachment

    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set;
    }

    public string NameWithoutExtension => Name[..^Extension.Length];
    public string Extension => GetExtension(Name);

    [JsonPropertyName("path")]
    public string Path
    {
        get;
        set;
    }

    public bool Download
    {
        get => _download;
        set => SetField(ref _download, value);
    }

    public bool UseRpc
    {
        get => _useRpc;
        set => SetField(ref _useRpc, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}