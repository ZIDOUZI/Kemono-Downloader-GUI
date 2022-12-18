namespace Kemono.Core.Contracts.Services;

public abstract class DownloadBase : IDownload<FileInfo>, IProgress<int>
{
    private long _length;

    private DownloadState _state;
    public abstract ValueTask DisposeAsync();
    public abstract void Dispose();

    public abstract Task Init();

    public abstract Task<FileInfo> Start();

    public abstract void Pause();
    public abstract void Stop();

    public virtual DownloadState State
    {
        get => _state;
        protected set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(value);
            }
        }
    }

    public bool Running => State is DownloadState.Initializing or DownloadState.Downloading;

    public event Action<int> ProgressChanged;
    public event Action<DownloadState> StateChanged;
    public event Action<long> LengthCallback;

    public long Length
    {
        get => _length;
        set
        {
            if (value != _length)
            {
                _length = value;
                LengthCallback?.Invoke(value);
            }
        }
    }

    public void Report(int value) => ProgressChanged?.Invoke(value);
}