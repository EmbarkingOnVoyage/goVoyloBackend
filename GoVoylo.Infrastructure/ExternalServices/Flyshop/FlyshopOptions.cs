namespace GoVoylo.Infrastructure.ExternalServices.Flyshop
{
    public class FlyshopOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        // AddPayment lives on a separate Flyshop service (tradehost/TradeAPIService.svc)
        // from every other call in this client (airlinehost/AirAPIService.svc), so it
        // needs its own base address rather than a relative path off BaseUrl.
        public string TradeBaseUrl { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string ImeiNumber { get; set; } = string.Empty;

        // Air_Reprice accepts a customer mobile number; the doc says to pass the
        // agency's own registered mobile number when there is no customer number yet.
        public string CustomerMobile { get; set; } = string.Empty;
    }
}
