using FluentAssertions;
using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Flights.Commands.ReleaseHold;
using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using NSubstitute;

namespace GoVoylo.Application.UnitTests.Features.Flights
{
    public class ReleaseHoldCommandHandlerTests
    {
        private readonly ITripBookingRepository _tripBookingRepository = Substitute.For<ITripBookingRepository>();
        private readonly IFlightSupplierClientResolver _resolver = Substitute.For<IFlightSupplierClientResolver>();
        private readonly IFlightSupplierClient _supplier = Substitute.For<IFlightSupplierClient>();
        private readonly ReleaseHoldCommandHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public ReleaseHoldCommandHandlerTests()
        {
            _resolver.Resolve(Arg.Any<string>()).Returns(_supplier);
            _handler = new ReleaseHoldCommandHandler(_resolver, _tripBookingRepository);
        }

        private TripBooking Hold(string? airlinePnr, Guid? userId = null)
        {
            var booking = new TripBooking(userId ?? _userId, "FLYSHOP", "REF1", airlinePnr, null, null, "33", 1000m, "INR", "Pax One", "1");
            _tripBookingRepository.GetByBookingRefNoAsync("REF1", Arg.Any<CancellationToken>()).Returns(booking);
            return booking;
        }

        [Fact]
        public async Task ReleasesWithTheSupplier_AndMarksTheBookingReleased()
        {
            var booking = Hold("PNR123");

            await _handler.Handle(new ReleaseHoldCommand(_userId, "REF1", "PNR123"), CancellationToken.None);

            await _supplier.Received(1).ReleaseHoldAsync(
                Arg.Is<SupplierReleaseHoldRequestDto>(r => r.BookingRefNo == "REF1" && r.AirlinePnr == "PNR123"),
                Arg.Any<CancellationToken>());
            booking.LocalStatus.Should().Be(TripBooking.StatusReleased);
            await _tripBookingRepository.Received(1).UpdateAsync(booking, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AHoldWithNoPnr_IsMarkedReleasedWithoutCallingTheSupplier()
        {
            var booking = Hold(null);

            await _handler.Handle(new ReleaseHoldCommand(_userId, "REF1", null), CancellationToken.None);

            await _supplier.DidNotReceiveWithAnyArgs().ReleaseHoldAsync(default!, default);
            booking.LocalStatus.Should().Be(TripBooking.StatusReleased);
        }

        [Fact]
        public async Task SomeoneElsesBooking_IsRefused()
        {
            var booking = Hold("PNR123", Guid.NewGuid());

            var act = () => _handler.Handle(new ReleaseHoldCommand(_userId, "REF1", "PNR123"), CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>();
            booking.LocalStatus.Should().Be(TripBooking.StatusActive);
            await _supplier.DidNotReceiveWithAnyArgs().ReleaseHoldAsync(default!, default);
        }

        [Fact]
        public async Task APaidBooking_IsLeftAlone()
        {
            var booking = Hold("PNR123");
            booking.RecordPayment(1000m);

            await _handler.Handle(new ReleaseHoldCommand(_userId, "REF1", "PNR123"), CancellationToken.None);

            booking.LocalStatus.Should().Be(TripBooking.StatusActive);
            await _supplier.DidNotReceiveWithAnyArgs().ReleaseHoldAsync(default!, default);
        }

        [Fact]
        public async Task AFailedSupplierRelease_KeepsTheBookingActive()
        {
            var booking = Hold("PNR123");
            _supplier.ReleaseHoldAsync(Arg.Any<SupplierReleaseHoldRequestDto>(), Arg.Any<CancellationToken>())
                .Returns<Task>(_ => throw new InvalidOperationException("supplier said no"));

            var act = () => _handler.Handle(new ReleaseHoldCommand(_userId, "REF1", "PNR123"), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            booking.LocalStatus.Should().Be(TripBooking.StatusActive);
        }
    }
}
