using Kemono.Core.Contracts.Services;

namespace Kemono.Core.Services;

public sealed class StreamDownload : IDownload<MemoryStream>
{
    private readonly string _url;
    private readonly HttpClient _client;
    private HttpResponseMessage _response;
    private readonly CancellationTokenSource _source = new();

    public StreamDownload(string url)
    {
        _url = url;
        _client = new HttpClient();
    }

    public StreamDownload(HttpClient client, string uri)
    {
        _client = client;
        _url = uri;
    }

    public void Dispose()
    {
        if (_response == null)
        {
            return;
        }

        try
        {
            _response.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public DownloadState State
    {
        get;
    }

    public bool Running
    {
        get;
    }

    public long Length
    {
        get;
    }

    public async Task Init()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _url);
        // request.Headers.
        _response = await _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);
        LengthCallback?.Invoke(_response.Content.Headers.ContentLength ?? 100);
    }

    public async Task<MemoryStream> Start()
    {
        await using var stream = await _response.Content.ReadAsStreamAsync();
        var builder = new MemoryStream();
        int len;
        var buffer = new byte[4096];
        while ((len = await stream.ReadAsync(buffer)) != 0)
        {
            ProgressChanged?.Invoke(len);
            await builder.WriteAsync(buffer.AsMemory(0, len));
        }

        return builder;
    }

    public void Pause()
    {
    }

    public void Stop() => _source.Cancel();

    public event Action<int> ProgressChanged;
    public event Action<DownloadState> StateChanged;
    public event Action<long> LengthCallback;
}