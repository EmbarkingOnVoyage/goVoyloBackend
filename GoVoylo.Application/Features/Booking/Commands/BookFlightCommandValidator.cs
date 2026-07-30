using FluentValidation;
using GoVoylo.Application.Features.Payments.Commands.ProcessPayment;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Booking.Commands
{
    public class BookFlightCommandValidator : AbstractValidator<BookFlightCommand>
    {
        public BookFlightCommandValidator()
        {
            RuleFor(x => x.NumberOfPassengers).GreaterThan(0).WithMessage("Passenger must be greater than zero.");
            RuleFor(x => x.From).NotEmpty().WithMessage("From must not be null");
            RuleFor(x => x.To).NotEmpty().WithMessage("To must not be empty");
            RuleFor(x => x.JourneyDate).NotEmpty().WithMessage("Date is mandatory");
        }
    }
}