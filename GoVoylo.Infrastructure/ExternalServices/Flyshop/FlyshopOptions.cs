namespace GoVoylo.Infrastructure.ExternalServices.Flyshop
{
    public class FlyshopOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string ImeiNumber { get; set; } = string.Empty;

        // Air_Reprice accepts a customer mobile number; the doc says to pass the
        // agency's own registered mobile number when there is no customer number yet.
        public string CustomerMobile { get; set; } = string.Empty;
    }
}
