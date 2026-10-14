namespace GoVoylo.Application.Features.Payments.Dtos
{
    // How a Razorpay payment was made: Method is Razorpay's own value (card, upi,
    // netbanking, wallet, emi, cardless_emi, paylater); Detail names the card
    // network/type and issuer, the bank or the wallet. No card numbers or UPI ids.
    public record RazorpayPaymentMethod(string Method, string? Detail);
}
