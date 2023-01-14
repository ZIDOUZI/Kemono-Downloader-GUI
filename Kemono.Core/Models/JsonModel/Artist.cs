using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Kemono.Core.Models.JsonModel;

public class Artist : INotifyPropertyChanged
{
    private bool? _download;
    private bool? _useRpc;

    public bool RpcEnable;

    [JsonPropertyName("favorited")]
    public long? Favorited
    {
        get;
        set;
    }

    [JsonPropertyName("id")]
    public string Id
    {
        get;
        set;
    }

    [JsonPropertyName("indexed")]
    public DateTimeOffset Indexed
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get;
        set;
    }

    [JsonPropertyName("service")]
    public Service Service
    {
        get;
        set;
    }

    [JsonPropertyName("update")]
    public DateTimeOffset Updated
    {
        get;
        set;
    }

    public IEnumerator<Post> Enumerator
    {
        get;
        private set;
    }

    public List<Post> Posts
    {
        get;
        private set;
    }

    public bool? Download
    {
        get => _download;
        set
        {
            if (SetField(ref _download, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public bool? UseRpc
    {
        get => _useRpc;
        set
        {
            if (SetField(ref _useRpc, value))
            {
                OnPropertyChanged(nameof(Text));
            }
        }
    }

    public string Text => Download switch
    {
        true => "全部下载",
        false => "不下载",
        null => "部分下载"
    } + "|" + UseRpc switch
    {
        true => "使用RPC",
        false => "不使用RPC",
        null => "部分使用RPC"
    };

    public string UseRpcText => RpcEnable ? "全部使用RPC" : "未启用RPC";

    public event PropertyChangedEventHandler PropertyChanged;

    public Artist SetPosts(params Post[] posts)
    {
        Posts = posts.ToList();
        Posts.ForEach(f =>
        {
            f.PropertyChanged += (_, _) =>
            {
                Download = Posts.All(it => it.Download == true)
                    ? true
                    : Posts.Any(it => it.Download == true)
                        ? null
                        : false;
                UseRpc = Posts.All(it => it.UseRpc == true)
                    ? true
                    : Posts.Any(it => it.UseRpc == true)
                        ? null
                        : false;
            };
        });
        Enumerator = Posts.GetEnumerator();
        return this;
    }

    internal Artist SetPosts(IEnumerable<Post> posts)
    {
        Posts = posts.ToList();
        Posts.ForEach(f =>
        {
            f.PropertyChanged += (_, _) =>
            {
                Download = Posts.All(it => it.Download == true)
                    ? true
                    : Posts.Any(it => it.Download == true)
                        ? null
                        : false;
                UseRpc = Posts.All(it => it.UseRpc == true)
                    ? true
                    : Posts.Any(it => it.UseRpc == true)
                        ? null
                        : false;
            };
        });
        Enumerator = Posts.GetEnumerator();
        return this;
    }

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