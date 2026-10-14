using FluentAssertions;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Features.Payments.Services;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Payments
{
    public class ETicketServiceTests
    {
        private readonly IFlightSupplierClientResolver _resolver = Substitute.For<IFlightSupplierClientResolver>();
        private readonly IFlightSupplierClient _supplier = Substitute.For<IFlightSupplierClient>();
        private readonly IETicketPdfGenerator _generator = Substitute.For<IETicketPdfGenerator>();
        private readonly ETicketService _service;
        private ETicketDocumentDto? _ticket;

        public ETicketServiceTests()
        {
            _resolver.Resolve(Arg.Any<string>()).Returns(_supplier);
            _generator.Generate(Arg.Do<ETicketDocumentDto>(t => _ticket = t)).Returns(Array.Empty<byte>());
            _service = new ETicketService(
                _resolver,
                Substitute.For<IUserRepository>(),
                _generator,
                Substitute.For<IEmailService>(),
                NullLogger<ETicketService>.Instance);
        }

        private void SupplierReturns(decimal baseFare, decimal taxes, decimal totalFare) =>
            _supplier.GetBookingDetailsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new SupplierBookingDetailsDto(
                    new[]
                    {
                        new SupplierBookingSegmentDto(0, "BOM", "GOI", "IX", "Air India Express", "1238",
                            new DateTime(2026, 11, 20, 8, 0, 0), new DateTime(2026, 11, 20, 9, 15, 0), 75)
                    },
                    new[] { new SupplierBookingPassengerDto("Mr", "Pax", "One", "Adult") },
                    baseFare,
                    taxes,
                    totalFare,
                    "INR",
                    Array.Empty<SupplierCancellationRuleDto>()));

        private static TripBooking Booking(decimal totalAmount) =>
            new(Guid.NewGuid(), "TRIPJACK", "TJS1", "PNR1", null, null, "11", totalAmount, "INR", "Pax One", "1");

        [Fact]
        public async Task AddOnsInsideTheSuppliersTotal_StillShowAsAddOns()
        {
            // Tripjack's total fare already includes the seats/meals/baggage booked.
            SupplierReturns(baseFare: 4035m, taxes: 3608m, totalFare: 20243m);

            await _service.BuildPdfAsync(Booking(20243m), CancellationToken.None);

            _ticket!.Payment.AddOns.Should().Be(12600m);
        }

        [Fact]
        public async Task AFareWithNothingAdded_HasNoAddOnsRow()
        {
            SupplierReturns(baseFare: 4035m, taxes: 3608m, totalFare: 7643m);

            await _service.BuildPdfAsync(Booking(7643m), CancellationToken.None);

            _ticket!.Payment.AddOns.Should().BeNull();
        }
    }
}
