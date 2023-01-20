using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Kemono.Core.Models.JsonModel;

public class Post : INotifyPropertyChanged
{
    private bool? _download;
    private bool? _useRpc;

    // TODO: 修改是否下载评论文字等
    public bool SaveContent = true;

    [JsonPropertyName("added")]
    public string Added
    {
        get;
        set;
    }

    // 附件
    [JsonPropertyName("attachments")]
    public List<WebFile> Attachments
    {
        get;
        set;
    }

    [JsonPropertyName("content")]
    public string Content
    {
        get;
        set;
    }

    [JsonPropertyName("edited")]
    public DateTimeOffset? Edited
    {
        get;
        set;
    }

    [JsonPropertyName("embed")]
    public Embed Embed
    {
        get;
        set;
    }

    [JsonPropertyName("file")]
    public WebFile File
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

    [JsonPropertyName("published")]
    public DateTimeOffset? Published
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

    [JsonPropertyName("shared_file")]
    public bool SharedFile
    {
        get;
        set;
    }

    [JsonPropertyName("title")]
    public string Title
    {
        get;
        set;
    }

    [JsonPropertyName("user")]
    public string User
    {
        get;
        set;
    }

    public long TimeStamp => Published?.Date.Date.ToUnixTimeMilliseconds() ?? -1;

    public List<WebFile> Files
    {
        get;
        private set;
    }

    public Uri Link => new($"https://kemono.party/{Service}/user/{User}/post/{Id}");

    public IEnumerator<WebFile> Enumerator
    {
        get;
        private set;
    }

    public bool? Download
    {
        get => _download;
        private set
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
        private set
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

    public event PropertyChangedEventHandler PropertyChanged;

    internal Post SetFiles(params WebFile[] files)
    {
        Files = files.ToList();
        Files.ForEach(f =>
        {
            f.PropertyChanged += (_, _) =>
            {
                Download = Files.Any(it => it.Download == false)
                    ? true
                    : Files.Any(it => it.Download)
                        ? null
                        : false;
                UseRpc = Files.Any(it => it.UseRpc == false)
                    ? true
                    : Files.Any(it => it.UseRpc)
                        ? null
                        : false;
            };
        });
        Enumerator = Files.GetEnumerator();
        return this;
    }

    internal Post SetFiles(IEnumerable<WebFile> files)
    {
        Files = files.ToList();
        Files.ForEach(f =>
        {
            f.PropertyChanged += (_, _) =>
            {
                Download = Files.Any(it => it.Download == false)
                    ? true
                    : Files.Any(it => it.Download)
                        ? null
                        : false;
                UseRpc = Files.Any(it => it.UseRpc == false)
                    ? true
                    : Files.Any(it => it.UseRpc)
                        ? null
                        : false;
            };
        });
        Enumerator = Files.GetEnumerator();

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