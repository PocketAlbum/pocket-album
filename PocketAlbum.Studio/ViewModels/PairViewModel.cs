using PocketAlbum.Server;
using PocketAlbum.Server.Controllers;

namespace PocketAlbum.Studio.ViewModels;

internal class PairViewModel : ViewModelBase
{
    public PairViewModel() : this(new TokenRequest() { ClientName = "Client" })
    {
        
    }

    public PairViewModel(TokenRequest request)
    {
        Request = request;
    }

    public TokenRequest Request { get; }
    public string Code { get; set; }
}
