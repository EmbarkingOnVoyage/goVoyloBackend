namespace GoVoylo.Application.Features.Payments.Dtos
{
    // Amount is in the smallest currency unit (paise for INR) — exactly what
    // Razorpay's Orders API returns and what the checkout SDK expects back.
    public record RazorpayOrderResult(string OrderId, long Amount, string Currency);
}
