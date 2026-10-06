using System.Globalization;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoVoylo.Application.Features.Payments.Services
{
    public class ETicketService : IETicketService
    {
        private readonly IFlightSupplierClientResolver _supplierClientResolver;
        private readonly IUserRepository _userRepository;
        private readonly IETicketPdfGenerator _pdfGenerator;
        private readonly IEmailService _emailService;
        private readonly ILogger<ETicketService> _logger;

        public ETicketService(
            IFlightSupplierClientResolver supplierClientResolver,
            IUserRepository userRepository,
            IETicketPdfGenerator pdfGenerator,
            IEmailService emailService,
            ILogger<ETicketService> logger)
        {
            _supplierClientResolver = supplierClientResolver;
            _userRepository = userRepository;
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendAsync(TripBooking booking, CancellationToken cancellationToken)
        {
            // Sent to the account's own email — the same address the booking was
            // made with in the app (and the hold email goes to).
            var user = await _userRepository.GetByIdAsync(booking.UserId);
            if (string.IsNullOrWhiteSpace(user?.Email))
            {
                _logger.LogWarning("No email on file for trip booking {TripBookingId}; e-ticket not sent.", booking.Id);
                return;
            }

            SupplierBookingDetailsDto? details = null;
            try
            {
                details = await _supplierClientResolver
                    .Resolve(booking.SupplierCode)
                    .GetBookingDetailsAsync(booking.BookingRefNo, booking.AirlinePnr, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The ticket still goes out, from what was stored at booking time.
                _logger.LogWarning(ex,
                    "Supplier booking details failed for trip booking {TripBookingId}; e-ticket built from stored data.",
                    booking.Id);
            }

            var phone = details?.ContactPhone
                ?? (string.IsNullOrWhiteSpace(user.Phone) ? null : $"{user.PhoneCountryCode}{user.Phone}");
            var ticket = Build(booking, details, details?.ContactEmail ?? user.Email, phone);
            var pdf = _pdfGenerator.Generate(ticket);

            await _emailService.SendETicketAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}".Trim(),
                booking.BookingRefNo,
                string.Join(", ", ticket.Trips.Select(t => RouteText(t.Segments))),
                pdf,
                $"GoVoylo-ETicket-{booking.BookingRefNo}.pdf");
        }

        private static ETicketDocumentDto Build(
            TripBooking booking, SupplierBookingDetailsDto? details, string? contactEmail, string? contactPhone)
        {
            var segments = details?.Segments ?? Array.Empty<SupplierBookingSegmentDto>();
            var passengers = details?.Passengers ?? Array.Empty<SupplierBookingPassengerDto>();

            // Trips: the supplier's when it answered, otherwise the stored legs.
            var trips = segments.Count > 0
                ? segments
                    .GroupBy(s => s.TripIndex)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Select(s => new ETicketSegmentDto(
                            s.AirlineCode,
                            s.AirlineName,
                            s.FlightNumber,
                            new ETicketAirportDto(s.Origin, s.OriginAirport?.City, s.OriginAirport?.Name, s.OriginAirport?.Terminal),
                            new ETicketAirportDto(s.Destination, s.DestinationAirport?.City, s.DestinationAirport?.Name, s.DestinationAirport?.Terminal),
                            s.DepartureDateTime,
                            s.ArrivalDateTime == default ? null : s.ArrivalDateTime,
                            s.DurationMinutes,
                            s.Stops))
                        .ToList())
                    .ToList()
                : booking.Legs
                    .Where(l => !l.IsCancelled)
                    .OrderBy(l => l.LegIndex)
                    .Select(l => new List<ETicketSegmentDto>
                    {
                        new(
                            l.AirlineCode,
                            l.AirlineName,
                            l.FlightNumber,
                            new ETicketAirportDto(l.Origin, null, null, null),
                            new ETicketAirportDto(l.Destination, null, null, null),
                            l.TravelDate,
                            null,
                            0,
                            0)
                    })
                    .ToList();

            var firstPassenger = passengers.FirstOrDefault();
            var tripDtos = trips
                .Select((tripSegments, index) =>
                {
                    var route = $"{tripSegments[0].From.Code}-{tripSegments[^1].To.Code}";
                    var pnr = passengers
                        .SelectMany(p => p.Tickets ?? Array.Empty<SupplierPassengerTicketDto>())
                        .FirstOrDefault(t => t.Route == route && !string.IsNullOrWhiteSpace(t.Pnr))?.Pnr
                        ?? booking.Legs.FirstOrDefault(l => l.LegIndex == index)?.AirlinePnr
                        ?? booking.AirlinePnr;
                    // A whole-trip offer is stored as one leg per trip; otherwise trip i is leg i.
                    var fareType = booking.Legs.FirstOrDefault(l => l.LegIndex == index)?.FareType
                        ?? booking.Legs.FirstOrDefault()?.FareType;
                    return new ETicketTripDto(
                        TripLabel(index, trips),
                        pnr,
                        CabinText(firstPassenger?.CabinClass),
                        BaggageText(firstPassenger?.CabinBaggage, firstPassenger?.CheckInBaggage),
                        tripSegments,
                        FareTypeLabel(fareType));
                })
                .ToList();

            var tripRoutes = tripDtos.Select(t => $"{t.Segments[0].From.Code}-{t.Segments[^1].To.Code}").ToList();
            string? TripPnr(string route) => tripDtos.ElementAtOrDefault(tripRoutes.IndexOf(route))?.Pnr
                ?? booking.AirlinePnr;
            var passengerDtos = passengers.Count > 0
                ? passengers
                    .Select(p => new ETicketPassengerDto(
                        $"{p.FirstName} {p.LastName}".Trim(),
                        p.PaxType,
                        (p.Tickets is { Count: > 0 }
                            ? p.Tickets.Select(t => new ETicketPassengerRouteDto(
                                t.Route, CabinText(p.CabinClass), t.Seat, t.Meal, t.ExtraBaggage, t.TicketNumber,
                                t.Pnr ?? TripPnr(t.Route)))
                            : tripRoutes.Select(r => new ETicketPassengerRouteDto(
                                r, CabinText(p.CabinClass), null, null, null, null, TripPnr(r))))
                        .ToList()))
                    .ToList()
                : booking.PassengerNames
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(name => new ETicketPassengerDto(
                        name,
                        "Adult",
                        tripRoutes.Select(r => new ETicketPassengerRouteDto(r, null, null, null, null, null, TripPnr(r))).ToList()))
                    .ToList();

            // One baggage row per passenger type and trip: "Adult", or "Each Adult (2)".
            var paxCounts = passengers.GroupBy(p => p.PaxType).ToDictionary(g => g.Key, g => g.Count());
            var baggage = (passengers.Count > 0
                    ? passengers.GroupBy(p => p.PaxType).Select(g => g.First())
                    : Array.Empty<SupplierBookingPassengerDto>())
                .SelectMany(p => tripDtos.Select(t => new ETicketBaggageRowDto(
                    paxCounts[p.PaxType] > 1 ? $"Each {p.PaxType} ({paxCounts[p.PaxType]})" : p.PaxType,
                    $"{t.Segments[0].From.Code}-{t.Segments[^1].To.Code} ({string.Join(", ", t.Segments.Select(s => $"{s.AirlineCode}-{s.FlightNumber}"))})",
                    string.IsNullOrWhiteSpace(p.CheckInBaggage) ? "–" : p.CheckInBaggage!,
                    string.IsNullOrWhiteSpace(p.CabinBaggage) ? "–" : p.CabinBaggage!)))
                .ToList();

            var addOns = details?.TotalFare is { } supplierTotal && booking.TotalAmount - supplierTotal >= 1m
                ? booking.TotalAmount - supplierTotal
                : (decimal?)null;

            var passengerSummary = passengerDtos.Count > 1
                ? string.Join(" + ", passengerDtos
                    .GroupBy(p => p.PaxType)
                    .Select(g => $"{g.Count()} {(g.Count() == 1 ? g.Key : PluralPaxType(g.Key))}"))
                : null;

            return new ETicketDocumentDto(
                booking.BookingRefNo,
                $"GV{booking.Id:N}"[..18].ToUpperInvariant(),
                TripTitle(tripDtos, segments),
                booking.CreatedAt,
                tripDtos,
                passengerDtos,
                baggage,
                new ETicketPaymentDto(details?.BaseFare, details?.TaxesAndFees, addOns, booking.TotalAmount, booking.CurrencyCode),
                contactEmail,
                contactPhone,
                tripDtos.Count > 2 || (tripDtos.Count == 2 && !IsRoundTrip(trips)),
                passengerSummary);
        }

        private static string PluralPaxType(string paxType) => paxType switch
        {
            "Child" => "Children",
            _ => paxType + "s"
        };

        // Supplier fare type → what the ticket prints. Tripjack's PUBLISHED is the
        // airline's standard fare; others read as their own name.
        private static string? FareTypeLabel(string? fareType)
        {
            if (string.IsNullOrWhiteSpace(fareType))
            {
                return null;
            }
            if (fareType.Equals("PUBLISHED", StringComparison.OrdinalIgnoreCase))
            {
                return "Regular Fare";
            }
            var words = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fareType.Replace('_', ' ').ToLowerInvariant());
            return words.EndsWith("Fare", StringComparison.Ordinal) ? words : $"{words} Fare";
        }

        private static string TripLabel(int index, IReadOnlyList<List<ETicketSegmentDto>> trips)
        {
            if (trips.Count == 1) return "Departure Flight";
            if (IsRoundTrip(trips)) return index == 0 ? "Departure Flight" : "Return Flight";
            return $"Flight {index + 1}";
        }

        private static bool IsRoundTrip(IReadOnlyList<IReadOnlyList<ETicketSegmentDto>> trips) =>
            trips.Count == 2 && trips[1][^1].To.Code == trips[0][0].From.Code;

        // "Domestic · One Way". Domestic/International only when every airport's
        // country is known.
        private static string TripTitle(IReadOnlyList<ETicketTripDto> trips, IReadOnlyList<SupplierBookingSegmentDto> segments)
        {
            var kind = trips.Count == 1
                ? "One Way"
                : IsRoundTrip(trips.Select(t => t.Segments).ToList()) ? "Round Trip" : "Multi-city";
            var countries = segments
                .SelectMany(s => new[] { s.OriginAirport?.CountryCode, s.DestinationAirport?.CountryCode })
                .ToList();
            if (countries.Count == 0 || countries.Any(string.IsNullOrWhiteSpace))
            {
                return kind;
            }
            return countries.All(c => string.Equals(c, "IN", StringComparison.OrdinalIgnoreCase))
                ? $"Domestic · {kind}"
                : $"International · {kind}";
        }

        private static string RouteText(IReadOnlyList<ETicketSegmentDto> segments) =>
            $"{segments[0].From.Code}–{segments[^1].To.Code}";

        // "ECONOMY" / "PREMIUM_ECONOMY" → "Economy" / "Premium Economy".
        private static string? CabinText(string? cabin) =>
            string.IsNullOrWhiteSpace(cabin)
                ? null
                : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cabin.Replace('_', ' ').ToLowerInvariant());

        private static string? BaggageText(string? cabin, string? checkIn)
        {
            var parts = new[] { cabin == null ? null : $"{cabin} cabin", checkIn == null ? null : $"{checkIn} check-in" }
                .Where(p => p != null);
            var text = string.Join(" / ", parts);
            return text.Length == 0 ? null : text;
        }
    }
}
