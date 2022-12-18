#nullable enable
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Aria2NET;
using Kemono.Core.Helpers;
using Kemono.Core.Models.JsonModel;
using Kemono.Core.Services;
using Microsoft.CSharp.RuntimeBinder;
using static System.DateTime;

namespace Kemono.Core.Models;

public enum UserAgent
{
    Edge,
    Chrome,
    BaiduNetdisk,
    Transmission,
    Aria2
}

public enum Domain
{
    kemono,
    commer
}

public class Downloader
{
    private const string Reg = "https://(.*)(kemono|commer)\\.party/(\\w+)/user/.+/?(post/.+)?";
    private const string _uuid = "88888888-4444-4444-4444-121212121212";

    private static readonly JsonSerializerOptions Options = new()
        {Converters = {new DateTimeOffsetFromNumberJsonConverter()}};

    private static readonly Regex ImgReg = new("<img src=\"([/a-z0-9.]+)\">");
    private static readonly Regex Wrong = new(@"[\s\.]+\\");
    private static readonly Regex UUID = new(@"\w{8}-\w{4}-\w{4}-\w{4}-\w{12}");
    private static readonly Regex NoFilename = new(@"\.+\\^");

    private readonly List<Artist> _artists = new();
    private readonly string _dataFormat;
    private readonly int _delay;

    private readonly string? _header;
    private readonly bool _overwrite;
    private readonly string _pattern;
    private readonly WebProxy? _proxy;
    private readonly uint _retry;
    private readonly Aria2? _rpc;
    public readonly string DefaultPath;
    public readonly bool LoggedIn;

    private Downloader(Builder builder, Action<Downloader>? init, Aria2? rpc, CookieCollection cookies, bool loggedIn,
        string? fallback = null)
    {
        var handler = new HttpClientHandler {UseCookies = true};
        var c = cookies.Where(it => it.Domain == $".{builder.Domain}.party").ToList();
        if (c.Any())
        {
            _header =
                $"Cookie:{string.Join(',', c.Select(it => it.ToString()))}\n" +
                $"User-Agent:{builder.UserAgent}\n" +
                "Connection:keep-alive";
        }

        if (builder.Proxy != "")
        {
            try
            {
                handler.Proxy = _proxy = new WebProxy(new Uri(builder.Proxy));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        handler.CookieContainer.Add(cookies);
        Client = new HttpClient(handler)
        {
            Timeout = builder.Timeout == 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(builder.Timeout),
            DefaultRequestHeaders =
                {{"User-Agent", builder.UserAgent}, {"Referrer", $"https://{builder.Domain}.party"}},
            BaseAddress = new Uri($"https://{builder.Domain}.party")
        };

        DefaultPath = string.IsNullOrWhiteSpace(builder.DefaultPath)
            ? fallback ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : builder.DefaultPath;
        _retry = builder.Retry;
        _delay = builder.Delay;
        _rpc = rpc;
        _pattern = builder.Pattern;
        _overwrite = builder.Overwrite;
        _dataFormat = builder.DateFormat;
        LoggedIn = loggedIn;

        init?.Invoke(this);
    }

    private HttpClient Client
    {
        get;
    }


    public bool HaveRpc => _rpc != null;
    public int ArtistCount => _artists.Count;

    public async Task DownloadArtists(Action<long> total, Action<int> added)
    {
        await using var download = new StreamDownload(Client, "api/creators");
        download.LengthCallback += total;
        download.ProgressChanged += added;
        await download.Init();
        var stream = await download.Start();
        var s = Encoding.UTF8.GetString(stream.ToArray());
        var json = JsonSerializer.Deserialize<List<Artist>>(s, Options);
        if (json != null)
        {
            _artists.AddRange(json);
        }
        else
        {
            Console.Write("error at get creators api info");
        }
    }

    public void AppendArtists(IEnumerable<Artist> artists) => _artists.AddRange(artists);

    private Artist GetArtist(Func<Artist, bool> selector) => _artists.First(selector);

    public List<Artist> GetArtists(Func<Artist, bool> query) => _artists.FindAll(x => query(x));

    public async Task<List<Artist>> GetFavoriteArtists() =>
        (await Client.GetFromJsonAsync<List<Artist>>("/api/v1/account/favorites?type=artist", Json.Options))!;

    public async Task<List<Post>> GetFavoritePosts() =>
        (await Client.GetFromJsonAsync<List<Post>>("/api/v1/account/favorites?type=post", Json.Options))!;

    public async Task<List<Post>> GetPosts(Artist a) => await GetPosts(a.Service, a.Id);

    public async Task<List<Post>> GetPosts(Service service, string id, int order = 0)
    {
        var l = new List<Post>();
        while (true)
        {
            
            var s = $"/api/{service}/user/{id}?o={order}";
            var receive = await Client.GetFromJsonAsync<List<Post>>(s, Json.Options);
            if (receive == null)
            {
                throw new NullReferenceException();
            }

            l.AddRange(receive);
            if (receive.Count == 0)
            {
                break;
            }

            order += receive.Count;
        }

        return l;
    }

    public async Task<Post> GetPost(Service service, string user, string id)
    {
        var order = 0;
        while (true)
        {
            var s = $"/api/{service}/user/{user}?o={order}";
            var receive = await Client.GetFromJsonAsync<List<Post>>(s, Json.Options);
            if (receive == null)
            {
                throw new NullReferenceException();
            }

            var result = receive.Find(post => post.Id == id);
            if (result != null)
            {
                return result;
            }

            if (receive.Count == 0)
            {
                break;
            }

            order += receive.Count;
        }

        throw new ArgumentException($"未找到此post(id={id})");
    }

    private string Format(Post post, int index = 0) =>
        Wrong.Replace(
            DefaultPath + _pattern
                .Replace("{service}", post.Service.ToString().EscapeFile())
                .Replace("{artist_id}", post.User.EscapeFile())
                .Replace("{artist}", Reverse(post).Name.EscapeFile())
                .Replace("{date}", post.Published?.ToString(_dataFormat) ?? "unknown time")
                .Replace("{title}", post.Title.EscapeFile())
                .Replace("{post_id}", post.Id.EscapeFile())
                .Replace("{time}", Now.ToString(_dataFormat))
                .Replace("{index0}", $"{index}")
                .Replace("{_index0}", $"_{index}")
                .Replace("{index0_}", $"{index}_")
                .Replace("{index}", index == 0 ? "" : $"{index}")
                .Replace("{_index}", index == 0 ? "" : $"_{index}")
                .Replace("{index_}", index == 0 ? "" : $"{index}_"), @"\");

    private string Format(Post post, WebFile file, int index = 0) =>
        NoFilename.Replace(
            Format(post, index)
                .Replace("{auto_named}",
                    UUID.IsMatch(file.NameWithoutExtension) ? index == 0 ? "cover" : $"{index}" : "{name}")
                .Replace("{name}",
                    file.NameWithoutExtension.Length > 256
                        ? $"{index}"
                        : file.NameWithoutExtension.EscapeFile()),
            match => match.Value + index
        )
        + file.Extension;

    private IEnumerable<WebFile> Filter(Post post, out bool haveCover)
    {
        var list = post.Attachments;
        foreach (Match match in ImgReg.Matches(post.Content))
        {
            var name = match.Groups[1].Value;
            list.Insert(0, new WebFile {Name = _uuid + Path.GetExtension(name), Path = match.Groups[1].Value});
        }

        haveCover = post.File.Path != null;
        if (haveCover)
        {
            list.Insert(0, post.File);
        }

        return list.TakeWhile(file => file?.Name != null).OnEach(file => file.UseRpc = HaveRpc);
    }

    public Artist Parse(Post post) =>
        GetArtist(a => a.Id == post.User).SetPosts(
            post.SetFiles(Filter(post, out var haveCover).Select((file, i) => file.Apply(f =>
                    {
                        f.File = Format(post, file, haveCover ? i : ++i);
                        f.Url = $"/data{file.Path!}";
                        f.UseRpc = HaveRpc;
                    })
                )
            )
        );

    public async Task<Artist> Parse(Artist artist) => artist.SetPosts(
        (await GetPosts(artist)).Select(post => post.SetFiles(
                Filter(post, out var haveCover).Select((file, i) => file.Apply(f =>
                    {
                        f.File = Format(post, file, haveCover ? i++ : i);
                        f.Url = $"/data{file.Path!}";
                        f.UseRpc = HaveRpc;
                    })
                )
            )
        )
    );

    /// <summary>
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="AmbiguousMatchException"></exception>
    public async Task<Artist> Parse(Uri url)
    {
        if (!Regex.IsMatch(url.OriginalString, Reg))
        {
            throw new AmbiguousMatchException();
        }

        var service = Enum.Parse<Service>(url.Segments[1].TrimEnd('/'));
        var user = url.Segments[3].TrimEnd('/');
        var artist = GetArtist(artist => artist.Id == user && artist.Service == service);

        return url.Segments.Length switch
        {
            4 => await Parse(artist),
            6 => Parse(await GetPost(service, user, url.Segments[5].TrimEnd('/'))),
            _ => throw new ArgumentException(url.OriginalString)
        };
    }

    public bool DownloadContent(Post post)
    {
        var file = new FileInfo(Format(post, new WebFile {Name = "Content.txt"}));
        if (!file.Directory!.Exists)
        {
            file.Directory.Create();
        }

        if (string.IsNullOrWhiteSpace(post.Content))
        {
            return false;
        }

        File.WriteAllText(file.FullName, post.Content, Encoding.UTF8);
        return true;
    }

    public async Task Favorite(Post post)
    {
        if ((await GetFavoritePosts()).Find(it => (it.Id == post.Id) & (it.Service == post.Service)) == null)
        {
            await Client.GetAsync($"/favorites/post/{post.Service}/{post.Id}");
        }
        else
        {
            await Client.DeleteAsync($"/favorites/post/{post.Service}/{post.Id}");
        }
    }

    public async Task Favorite(Artist artist)
    {
        if ((await GetFavoriteArtists()).Find(it => (it.Id == artist.Id) & (it.Service == artist.Service)) == null)
        {
            await Client.GetAsync($"/favorites/artist/{artist.Service}/{artist.Id}");
        }
        else
        {
            await Client.DeleteAsync($"/favorites/post/{artist.Service}/{artist.Id}");
        }
    }

    public FileDownload? Download(WebFile info, long buffer = 8192) =>
        new FileInfo(info.File).Exists && !_overwrite
            ? null
            : new FileDownload(Client, info.Url, info.File, buffer, _retry);

    public async Task<string> Post(WebFile info, bool useProxy = false)
    {
        var dict = new Dictionary<string, string>();
        if (useProxy && _proxy != null && _proxy.Address != null)
        {
            dict["all-proxy"] = _proxy.Address!.ToString();
            if (_proxy.Credentials is NetworkCredential c)
            {
                dict["all-proxy-user"] = c.UserName;
                dict["all-proxy-passwd"] = c.Password;
            }
        }

        if (_header != null)
        {
            dict["header"] = _header;
        }

        dict["referer"] = "*";
        dict["allow-overwrite"] = _overwrite ? "true" : "false";

        var file = new FileInfo(info.File);
        await Task.Delay(_delay);
        return await _rpc!.AddUri(info.Url, file.DirectoryName, file.Name, dict);
    }

    private Artist Reverse(Post input) => _artists.First(it => it.Id == input.User);

    public class Builder
    {
        private static readonly HttpClientHandler Handler = new() {AllowAutoRedirect = false, UseCookies = true};

        private readonly HttpClient _client = new(Handler);

        private readonly CookieCollection _cookies = new();
        private bool _loggedIn;
        private Aria2? _rpc;
        public string DateFormat = "yyyy-MM-dd HH-mm";
        public string DefaultPath = "";
        public int Delay = 0;

        public bool Overwrite = false;
        public string Pattern = @"\kemono\{service}\{artist}[{artist_id}]\[{date}]{title}[{post_id}]\{name}{_index}";
        public string Proxy = "";
        public uint Retry = 10;

        public long Timeout = 0;
        public string UserAgent = "aria2/1.35.0";
        public bool UseRpc = false;

        public Builder(Action<Builder>? init = null)
        {
            init?.Invoke(this);
        }

        public Domain Domain = Domain.kemono;

        public async Task<bool> Aria2Config(string uri, string token = "")
        {
            var host = new Uri(uri);
            if (!uri.EndsWith("jsonrpc"))
            {
                host = new Uri(host, "jsonrpc");
            }

            _rpc = new Aria2(host, token);
            return await _rpc.CheckAvailability();
        }

        public async Task<bool> Login(string username, string password)
        {
            var body = new FormUrlEncodedContent(
                new Dictionary<string, string> {{"username", username}, {"password", password}}
            );

            var result = await _client.PostAsync($"https://{Domain}.party/account/login", body);
            if (result.StatusCode != HttpStatusCode.Redirect)
            {
                throw new ApplicationException();
            }

            if (result.Headers.Location!.OriginalString != "/artists?logged_in=yes")
            {
                return false;
            }

            try
            {
                foreach (var cookieString in result.Headers.GetValues("set-cookie"))
                {
                    _cookies.Add(GetCookiesByHeader(cookieString, $"{Domain}.party"));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
                return true;
            }

            _loggedIn = true;
            return true;
        }

        private static CookieCollection GetCookiesByHeader(string setCookie, string domain)
        {
            var cookieCollection = new CookieCollection();
            setCookie += ",T"; //配合RegexSplitCookie2 加入后缀
            var listStr = new Regex(@"[^,][\S\s]+?;+[\S\s]+?(?=,\S)").Matches(setCookie);
            //循环遍历
            foreach (Match item in listStr)
            {
                //根据; 拆分Cookie 内容
                var cookieItem = item.Value.Split(';');
                var cookie = new Cookie();
                for (var index = 0; index < cookieItem.Length; index++)
                {
                    var info = cookieItem[index];
                    //第一个 默认 Cookie Name
                    //判断键值对
                    if (info.Contains('='))
                    {
                        var i = info.IndexOf('=');
                        var key = info[..i].Trim();
                        var value = info[(i + 1)..];
                        if (index == 0)
                        {
                            cookie.Name = key;
                            cookie.Value = value;
                            continue;
                        }

                        if (key.Equals("Domain", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Domain = value;
                        }
                        else if (key.Equals("Expires", StringComparison.OrdinalIgnoreCase) &&
                                 TryParse(value, out var expires))
                        {
                            cookie.Expires = expires;
                        }
                        else if (key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Path = value;
                        }
                        else if (key.Equals("Version", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Version = Convert.ToInt32(value);
                        }
                    }
                    else if (info.Trim().Equals("HttpOnly", StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.HttpOnly = true;
                    }
                }

                if (cookie.Domain == "")
                {
                    cookie.Domain = domain;
                }

                cookieCollection.Add(cookie);
            }

            return cookieCollection;
        }

        public async Task<Downloader> Build(Action<Downloader>? init = null)
        {
            if (!_cookies.Any())
            {
                try
                {
                    var response = await _client.GetAsync($"https://{Domain}.party/");
                    foreach (var cookie in response.Headers.GetValues("set-cookie"))
                    {
                        _cookies.Add(GetCookiesByHeader(cookie, $"{Domain}.party"));
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            if ((_rpc == null) | !UseRpc)
            {
                return new Downloader(this, init, _rpc, _cookies, _loggedIn);
            }

            try
            {
                var json = JsonSerializer.Deserialize<JsonNode>(await _rpc!.GetGlobalOption());
                var path = json!["result"]!["dir"]!.GetValue<string>();
                _rpc.BaseUri = new Uri($"https://{Domain}.party/");
                // _rpc
                return new Downloader(this, init, _rpc, _cookies, _loggedIn, path);
            }
            catch (Exception)
            {
                return new Downloader(this, init, _rpc, _cookies, _loggedIn);
            }
        }
    }
}