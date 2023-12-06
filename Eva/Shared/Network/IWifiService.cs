namespace Eva.Shared.Network
{
    public interface IWifiService
    {
        Task<List<Eva.Models.Network>> GetAvailableNetworksAsync();
    }
}
