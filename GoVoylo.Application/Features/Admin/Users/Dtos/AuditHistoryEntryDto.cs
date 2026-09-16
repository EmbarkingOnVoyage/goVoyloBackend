namespace GoVoylo.Application.Features.Admin.Users.Dtos
{
    public record AuditHistoryEntryDto(string EventType, Guid? ActorUserId, DateTime CreatedAt);
}
