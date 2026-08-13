namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record AddressDto(
        Guid Id,
        string? Label,
        string Line1,
        string? Line2,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault);
}
