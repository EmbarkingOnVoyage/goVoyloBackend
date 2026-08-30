// GoVoylo.Application/Interfaces/ITripJackTestService.cs
namespace GoVoylo.Application.Interfaces;

public interface ITripJackTestService
{
    Task<string> ExecuteRawSearchAsync(string origin, string destination, DateTime departureDate, CancellationToken ct);
}
