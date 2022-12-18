#nullable enable
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using Kemono.Core.Contracts.Services;

namespace Kemono.Core.Services;

public sealed class FileDownload : DownloadBase
{
    private readonly byte[] _buff;
    private readonly HttpClient _client;
    private readonly string _path;
    private readonly CancellationTokenSource _source = new();
    private readonly FileStream? _stream;
    private readonly string _url;
    private long _position;
    private HttpResponseMessage? _response;
    private uint _retry;

    public FileDownload(HttpClient client, string uri, string path, long buffer = 8192, uint retry = 3)
    {
        _client = client;
        _path = path;
        _buff = new byte[buffer];
        _retry = retry;
        ProgressChanged += i => _position += i;
        _url = uri;

        var file = new FileInfo($"{_path}.part");
        if (file.Exists)
        {
            _position = file.Length;
            _stream = file.OpenWrite();
        }
        else if (file.Directory!.Exists)
        {
            _stream = file.Create();
        }
        else
        {
            file.Directory!.Create();
            _stream = file.Create();
        }
    }

    private CancellationToken Token => _source.Token;

    public override void Dispose()
    {
        _source.Dispose();
        _response?.Dispose();
        _stream?.Flush();
        _stream?.Close();
        Console.WriteLine($"I: Disposed {Path.GetFileName(_path)}.");
    }

    public override ValueTask DisposeAsync()
    {
        _source.Dispose();
        _response?.Dispose();
        _stream?.Flush();
        Console.WriteLine($"I: Disposed {Path.GetFileName(_path)} async.");
        return _stream?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    public async override Task Init()
    {
        State = DownloadState.Initializing;
        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri(_url, UriKind.Relative),
            Method = HttpMethod.Get,
            Headers = {{"Range", $"bytes={_position}-"}}
        };
        _response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Token);

        switch (_response.StatusCode)
        {
            case HttpStatusCode.Forbidden:
                throw new UnauthorizedAccessException();
            case HttpStatusCode.RequestedRangeNotSatisfiable:
                throw new NetworkInformationException(416);
            case HttpStatusCode.BadGateway:
                throw new NetworkInformationException(503);
            default:
                Length = (long)_response.Content.Headers.ContentLength!;
                State = DownloadState.Initialized;
                return;
        }
    }

    public async override Task<FileInfo?> Start()
    {
        State = DownloadState.Downloading;

        if (Length == default)
        {
            return null;
        }

        Exception? exception = null;
        while (_retry-- != 0)
        {
            try
            {
                if (_position == Length)
                {
                    State = DownloadState.Succeed;
                    await DisposeAsync();
                    new FileInfo($"{_path}.part").MoveTo(_path);
                    return new FileInfo(_path);
                }

                _stream!.Position = _position;
                await using var stream = await _response!.Content.ReadAsStreamAsync(Token);
                int len;

                while ((len = await stream.ReadAsync(_buff, Token)) != 0)
                {
                    if (State != DownloadState.Downloading)
                    {
                        return null;
                    }

                    Report(len);
                    await _stream.WriteAsync(_buff.AsMemory(0, len), Token);
                }

                await _stream.FlushAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debugger.Break();
                exception = e;
            }
        }

        Debugger.Break();
        State = DownloadState.Faulted;
        throw new EndOfStreamException($"total length is {Length}, while all receive is {_position}", exception);
    }

    public override void Pause() => State = DownloadState.Paused;

    public override void Stop() => State = DownloadState.Canceled;
}