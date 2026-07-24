namespace GoVoylo.Domain.Entities
{
    public class AnalyticsTelemetry
    {
        public string EventName { get; private set; }
        public string ClientType { get; private set; }
        public DateTime LoggedAt { get; private set; }
        public Dictionary<string, string> MetaData { get; private set; }
        public AnalyticsTelemetry(string eventName, string clientType, Dictionary<string, string> metaData)
        {
            EventName = eventName;
            ClientType = clientType;
            LoggedAt = DateTime.UtcNow;
            MetaData = metaData ?? new Dictionary<string, string>();
        }
    }
}