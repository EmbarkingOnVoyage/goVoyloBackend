namespace GoVoylo.Application.Interfaces
{
    // Records where sign-ins and payment attempts come from (IP, location, IP
    // safety, payment mode). Non-blocking: the caller's request never waits on
    // the IP lookup or the payment-mode fetch — a background worker does both.
    public interface IAccessAuditService
    {
        // method: otp | password | google | facebook | apple
        void LogSignIn(Guid userId, string method);

        void LogPaymentStarted(Guid? userId, string bookingReference, string gatewayOrderId, decimal amount, string currency);

        // status: Succeeded | Failed. The worker asks Razorpay how it was paid.
        void LogPaymentCompleted(string gatewayOrderId, string? gatewayPaymentId, string status);
    }
}
