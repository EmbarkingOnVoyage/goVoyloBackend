namespace GoVoylo.Infrastructure.Jobs
{
    public class UnpaidHoldExpiryOptions
    {
        // An unpaid hold older than this is an abandoned checkout (tab or app
        // closed during Razorpay). Kept well above any real checkout time, since
        // a payment verified after the release can't be ticketed.
        public int UnpaidHoldMinutes { get; set; } = 120;
        public int RunIntervalMinutes { get; set; } = 15;
    }
}
