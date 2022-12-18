using System.Collections.ObjectModel;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Models;

namespace Kemono.ViewModels;

public class DownloadViewModel : InjectScopeViewModel
{
    private ObservableCollection<Artist> _artists = new();

    public Downloader Downloader = null!;

    public ObservableCollection<Artist> Artists
    {
        get => _artists;
        set => SetProperty(ref _artists, value);
    }
}