using Kemono.Core.Contracts.Services;

namespace Kemono.Core.Services;

public sealed class MegaDownload : IFileDownload
{
    Task IFileDownload.Init() => throw new NotImplementedException();

    public Task Start() => throw new NotImplementedException();

    public void Pause() => throw new NotImplementedException();

    public void Stop() => throw new NotImplementedException();

    public DownloadState State
    {
        get;
    }

    public bool Running
    {
        get;
    }

    public event Action<int> ProgressChanged;
    public event Action<DownloadState> StateChanged;
    public event Action<long> LengthCallback;

    public long Length
    {
        get;
    }

    public void Dispose() => throw new NotImplementedException();
    public ValueTask DisposeAsync() => throw new NotImplementedException();
}