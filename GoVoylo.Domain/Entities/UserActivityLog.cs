using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities;
public class UserActivityLog : BaseEntity
{
    public string UserId { get; private set; }
    public string ActionType { get; private set; } // e.g., Login, Logout, BookingCreated
    public string PayloadJson { get; private set; }
    public string SourcePlatform  { get; private set; } // JSON or any other format for additional info

    public UserActivityLog(string userId, string actionType, string payloadJson, string sourcePlatform)
    {
        if (string.IsNullOrWhiteSpace(userId)) 
            throw new ArgumentException("User ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(actionType)) 
            throw new ArgumentException("Action type cannot be empty.");

        UserId = userId;
        ActionType = actionType;
        PayloadJson = payloadJson;
        SourcePlatform = sourcePlatform;
    }
}