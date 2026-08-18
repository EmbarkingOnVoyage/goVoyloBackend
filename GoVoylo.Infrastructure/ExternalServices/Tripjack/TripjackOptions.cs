namespace GoVoylo.Infrastructure.ExternalServices.Tripjack
{
    public class TripjackOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string ImeiNumber { get; set; } = string.Empty;
    }
}
