// DTOs define the contract between API controllers/agents and application logic. 
// They are flat, serializable data structures with no business logic.
namespace GoVoylo.Application.Features.Payments.Dtos
{
    public record PaymentResponseDto
                (Guid TransactionId,
                string ReferenceNumber,
                decimal Amount,
                string Currency,
                string Status,
                DateTime CreatedAt);
}