namespace GoVoylo.Domain.Interfaces;

public interface ITravelSupplierClient
{
    // Fetches live external data for your web/mobile consumers and AI agent
    Task<string> GetLiveOffersAsync(string searchParametersJson);
}