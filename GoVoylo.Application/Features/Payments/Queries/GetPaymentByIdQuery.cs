using MediatR;
using GoVoylo.Application.Features.Payments.Dtos;

namespace GoVoylo.Application.Features.Payments.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDetailsDto?>;
