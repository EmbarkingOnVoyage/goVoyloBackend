using GoVoylo.Application.Features.Payments.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Payments.Commands.VerifyPayment
{
    public record VerifyPaymentCommand(
    Guid PaymentId,
    string PaymentProvider,
    string ProviderOrderId,
    string ProviderPaymentId,
    string ProviderSignature
) : IRequest<VerifyPaymentResponseDto>;
}
