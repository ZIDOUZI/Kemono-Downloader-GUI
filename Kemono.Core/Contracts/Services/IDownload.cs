namespace Kemono.Core.Contracts.Services;

public interface IDownload<T> : IDisposable, IAsyncDisposable
{
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

    public Task Init();
    public Task<T> Start();
    public void Pause();
    public void Stop();

    public event Action<int> ProgressChanged;

    public event Action<DownloadState> StateChanged;

    public event Action<long> LengthCallback;
}

public interface IFileDownload : IDisposable, IAsyncDisposable
{
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

    public Task Init();
    public Task Start();
    public void Pause();
    public void Stop();

    public event Action<int> ProgressChanged;

    public event Action<DownloadState> StateChanged;

    public event Action<long> LengthCallback;
}
public enum DownloadState
{
    Created,
    Initializing,
    Initialized,
    Downloading,
    Paused,
    Faulted,
    Canceled,
    Succeed
}