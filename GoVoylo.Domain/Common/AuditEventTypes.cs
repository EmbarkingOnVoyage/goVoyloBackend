namespace GoVoylo.Domain.Common
{
    public static class AuditEventTypes
    {
        public const string Registration = "registration";
        public const string LoginSuccess = "login_success";
        public const string LoginFailed = "login_failed";
        public const string Logout = "logout";
        public const string PasswordChanged = "password_changed";
        public const string CustomerStatusChanged = "customer_status_changed";
    }
}
