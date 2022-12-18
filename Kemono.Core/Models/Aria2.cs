#nullable enable
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kemono.Core.Models;

//Aria2详细信息请看
//https://aria2.github.io/manual/en/html/aria2c.html#rpc-interface

//定义一个类用于转码成JSON字符串发送给Aria2 json-rpc接口
public class JsonClass
{
    public string Id
    {
        get;
        init;
    } = Guid.NewGuid().ToString();

    public string Method
    {
        get;
        init;
    } = "aria2.addUri";

    public List<string> Params
    {
        get;
        set;
    } = new();
}

public class Aria2
{
    private readonly HttpClient _client = new(); //用于连接到Aria2Rpc的客户端
    private readonly Uri _host;
    public Uri? BaseUri;
    private readonly string _token;

    /// <summary>
    ///     连接Aria2Rpc服务器
    /// </summary>
    /// <param name="uri">aria2地址</param>
    /// <param name="token">aria2 token, 默认为空</param>
    /// <returns></returns>
    public Aria2(Uri uri, string token = "")
    {
        _host = uri;
        _token = token;
    }

    public async Task<bool> CheckAvailability()
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonNode>(await GetGlobalOption());
            return json?["result"] != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     JsonClass类转为json格式
    /// </summary>
    /// <param name="json"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    private string ToJson(JsonClass json, Dictionary<string, string>? config = null)
    {
        var @params = new JsonArray();
        if (_token != "")
        {
            @params.Add($"token:{_token}");
        }

        @params.Add(JsonSerializer.SerializeToNode(json.Params));
        @params.Add(JsonSerializer.SerializeToNode(config ?? new Dictionary<string, string>()));
        var s = new JsonObject
        {
            { "jsonrpc", "2.0" },
            { "id", json.Id },
            { "method", json.Method },
            { "params", @params }
        };
        return s.ToJsonString();
    }


    /// <summary>
    ///     发送json并返回json消息
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private async Task<string> Send(string str)
    {
        try
        {
            var request = new HttpRequestMessage
            {
                RequestUri = _host,
                Method = HttpMethod.Post,
                Content = new StringContent(str, Encoding.UTF8, "application/json")
            };
            var response = await _client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            Debugger.Break();
            return @"{""id"":""null"",""error"":""Connection Failed""}";
        }
    }


    /// <summary>
    ///     添加新的下载
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="dir"></param>
    /// <param name="out"></param>
    /// <param name="otherParams"></param>
    /// <returns></returns>
    public async Task<string> AddUri(string uri, string? dir = null, string? @out = null,
        Dictionary<string, string>? otherParams = null)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.addUri"
        };
        //添加下载地址
        var @params = new List<string> { Uri.TryCreate(BaseUri, uri, out var fullUri) ? fullUri.AbsoluteUri : uri };
        var dict = otherParams ?? new Dictionary<string, string>();
        if (dir != null)
        {
            dict.Add("dir", dir);
        }

        if (@out != null)
        {
            dict.Add("out", @out);
        }

        json.Params = @params;
        return await Send(ToJson(json, dict));
    }


    /// <summary>
    ///     上传“.torrent”文件添加BitTorrent下载
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<string> AddTorrent(string path)
    {
        var fs = await File.ReadAllBytesAsync(path);
        var base64 = Convert.ToBase64String(fs); //转为Base64编码

        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.addTorrent",
            //添加“.torrent”文件本地地址
            Params = new List<string> { base64 }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     删除已经停止的任务，强制删除请使用ForceRemove方法
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> Remove(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.remove",
            //添加下载地址
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     强制删除
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> ForceRemove(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.forceRemove",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     暂停下载,强制暂停请使用ForcePause方法
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> Pause(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.pause",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     暂停全部任务
    /// </summary>
    /// <returns></returns>
    public async Task<string> PauseAll()
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.pauseAll"
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     强制暂停下载
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> ForcePause(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.forcePause",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     强制暂停全部下载
    /// </summary>
    /// <returns></returns>
    public async Task<string> ForcePauseAll()
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.forcePauseAll"
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     把正在下载的任务状态改为等待下载
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> PauseToWaiting(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.unpause",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     把全部在下载的任务状态改为等待下载
    /// </summary>
    /// <returns></returns>
    public async Task<string> PauseToWaitingAll()
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.unpauseAll"
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     返回下载进度
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> TellStatus(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.tellStatus",
            Params = new List<string>
            {
                gid,
                "completedLength\", \"totalLength\",\"downloadSpeed"
            }
        };

        return await Send(ToJson(json));
    }


    /// <summary>
    ///     返回全局统计信息，例如整体下载和上载速度
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> GetGlobalStat(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.getGlobalStat",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     返回全局设置
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetGlobalOption()
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.getGlobalOption"
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     返回由gid（字符串）表示的下载中使用的URI 。响应是一个结构数组
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> GetUris(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.getUris",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }


    /// <summary>
    ///     返回由gid（字符串）表示的下载文件列表
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    public async Task<string> GetFiles(string gid)
    {
        var json = new JsonClass
        {
            Id = Guid.NewGuid().ToString(),
            Method = "aria2.getFiles",
            Params = new List<string> { gid }
        };
        return await Send(ToJson(json));
    }
}