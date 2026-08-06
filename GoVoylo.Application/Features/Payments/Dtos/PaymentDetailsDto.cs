namespace GoVoylo.Application.Features.Payments.Dtos;

public record PaymentDetailsDto(
    Guid Id,
    string BookingReference,
    decimal TotalAmount,
    string Currency
);
