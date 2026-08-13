namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record GstDetailsDto(
        Guid Id,
        string Gstin,
        string LegalName,
        string? TradeName,
        bool IsVerified);
}
