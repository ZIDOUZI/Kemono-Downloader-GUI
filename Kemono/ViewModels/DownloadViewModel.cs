using System.Collections.ObjectModel;
using Downloader;
using Kemono.Contracts.Models;
using Kemono.Core.Models;
using Kemono.Core.Models.JsonModel;
using Kemono.Models;
using Kemono.Models.Tree;
using Microsoft.UI.Xaml.Controls;

namespace Kemono.ViewModels;

public class DownloadViewModel : InjectScopeViewModel
{
    // private ObservableCollection<Artist> _artists = new();

    public Resolver Resolver = null!;
    private ObservableCollection<ArtistUI> _resolved = new();
    public bool HaveRpc;

    public ObservableCollection<ArtistUI> Resolved
    {
        get => _resolved;
        set => SetProperty(ref _resolved, value);
    }

    // public ObservableCollection<Artist> Artists
    // {
    //     get => _artists;
    //     set => SetProperty(ref _artists, value);
    // }
}