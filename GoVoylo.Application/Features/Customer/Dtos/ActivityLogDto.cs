namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record ActivityLogDto(
        string ActionType,
        string PayloadJson,
        string SourcePlatform,
        DateTime CreatedAt);
}
