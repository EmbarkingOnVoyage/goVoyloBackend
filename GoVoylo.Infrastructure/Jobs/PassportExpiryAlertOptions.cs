namespace GoVoylo.Infrastructure.Jobs
{
    public class PassportExpiryAlertOptions
    {
        public int WindowDays { get; set; } = 90;
        public int RunIntervalHours { get; set; } = 24;
    }
}
