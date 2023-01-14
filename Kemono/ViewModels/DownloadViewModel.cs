using System.Collections.ObjectModel;
using Downloader;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Models;

namespace Kemono.ViewModels;

public class DownloadViewModel : InjectScopeViewModel
{
    public static DownloadConfiguration Option = new()
    {

    };
    private ObservableCollection<Artist> _artists = new();

    public Resolver Resolver = null!;

    public ObservableCollection<Artist> Artists
    {
        get => _artists;
        set => SetProperty(ref _artists, value);
    }
}