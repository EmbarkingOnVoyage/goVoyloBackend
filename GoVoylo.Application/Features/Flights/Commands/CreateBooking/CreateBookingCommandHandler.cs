using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Commands.CreateBooking
{
    // Creates a Flyshop temp booking across every leg and immediately places a
    // Block_Ticket hold on it (see IFlightSupplierClient.CreateBlockTicketAsync
    // for why this never calls Book_Ticket). The hold is reversible via
    // Air_ReleasePNR (not yet implemented) rather than a final purchase.
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponseDto>
    {
        private readonly IFlightSupplierClient _supplierClient;
        private readonly IFlightSearchSessionStore _sessionStore;

        public CreateBookingCommandHandler(
            IFlightSupplierClient supplierClient,
            IFlightSearchSessionStore sessionStore)
        {
            _supplierClient = supplierClient;
            _sessionStore = sessionStore;
        }

        public async Task<CreateBookingResponseDto> Handle(
            CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var bookingFlights = new List<SupplierBookingFlightDto>();

            foreach (var leg in request.Legs)
            {
                var session = await _sessionStore.GetAsync(leg.OfferId, cancellationToken);

                if (session == null)
                {
                    throw new NotFoundException("Flight offer not found or has expired. Please search again.");
                }

                // Same reasoning as GetFlightAncillariesQueryHandler/GetSeatMapQueryHandler:
                // Air_TempBooking needs the Flight_Key from a fresh Air_Reprice response.
                var repriceResult = await _supplierClient.RepriceAsync(
                    new SupplierRepriceRequestDto(session.SearchKey, session.FlightKey, session.FareId),
                    cancellationToken);

                var updatedSession = session with
                {
                    FlightKey = repriceResult.FlightKey,
                    FareId = repriceResult.FareId
                };
                await _sessionStore.UpdateAsync(leg.OfferId, updatedSession, cancellationToken);

                bookingFlights.Add(new SupplierBookingFlightDto(
                    session.SearchKey,
                    updatedSession.FlightKey,
                    leg.SelectedSsrs
                        .Select(s => new SupplierBookingSsrDto(s.PaxId, s.SsrKey))
                        .ToList()));
            }

            var travelers = request.Travelers
                .Select(t => new SupplierTempBookingPaxDto(
                    t.PaxId, MapPaxType(t.PaxType), t.Title, t.FirstName, t.LastName, MapGender(t.Gender)))
                .ToList();

            var tempBooking = await _supplierClient.CreateTempBookingAsync(
                new SupplierTempBookingRequestDto(
                    request.PassengerMobile, request.PassengerEmail, travelers, bookingFlights),
                cancellationToken);

            var ticket = await _supplierClient.CreateBlockTicketAsync(tempBooking.BookingRefNo, cancellationToken);

            return new CreateBookingResponseDto(
                ticket.BookingRefNo,
                ticket.StatusId,
                ticket.AirlineCode,
                ticket.AirlinePnr,
                ticket.RecordLocator,
                ticket.FailureRemark);
        }

        // 0-ADT/1-CHD/2-INF, matching Flyshop's own FareDetails.PAX_Type convention.
        private static int MapPaxType(string paxType) => paxType switch
        {
            "Child" => 1,
            "Infant" => 2,
            _ => 0
        };

        // 0-Male/1-Female — the only two values Flyshop's PAX_Details documents.
        private static int MapGender(string gender) => gender switch
        {
            "Female" => 1,
            _ => 0
        };
    }
}
