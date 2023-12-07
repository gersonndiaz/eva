namespace Eva.Shared.Network
{
    public interface IWifiService
    {
        Task<List<Models.App.Network>> GetAvailableNetworksAsync();
    }
}
