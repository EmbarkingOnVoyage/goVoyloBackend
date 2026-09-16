// GoVoylo.Application/Features/B2bIntegrations/Queries/TestTripJackSearch/TestTripJackSearchQuery.cs
using MediatR;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Application.Features.B2bIntegrations.Queries.TestTripJackSearch;

// The request sent from your API Controller
public record TestTripJackSearchQuery(
    string Origin, 
    string Destination, 
    DateTime DepartureDate
) : IRequest<string>;

// The Handler that executes the use case
public class TestTripJackSearchQueryHandler : IRequestHandler<TestTripJackSearchQuery, string>
{
    private readonly ITripJackTestService _tripJackTestService;

    // Inject the testing interface here
    public TestTripJackSearchQueryHandler(ITripJackTestService tripJackTestService)
    {
        _tripJackTestService = tripJackTestService;
    }

    public async Task<string> Handle(TestTripJackSearchQuery request, CancellationToken cancellationToken)
    {
        // Execute the raw search via the Infrastructure layer implementation
        var rawResult = await _tripJackTestService.ExecuteRawSearchAsync(
            request.Origin, 
            request.Destination, 
            request.DepartureDate, 
            cancellationToken);

        return rawResult;
    }
}
