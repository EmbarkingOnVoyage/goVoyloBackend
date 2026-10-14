namespace GoVoylo.Application.Features.Pricing.Services
{
    // The commission discount (see CommissionDiscountRate). A fare's list price is
    // the supplier's price plus our commission (shown struck out); the customer is
    // charged the list price minus Percent of the commission. At 100% that's the
    // supplier's own price; below it, the commission not given back is charged on
    // top of the supplier's price.
    public static class CommissionPricing
    {
        public static decimal Discount(decimal commission, decimal percent) =>
            commission <= 0m ? 0m : Math.Round(commission * Math.Clamp(percent, 0m, 100m) / 100m, 2, MidpointRounding.AwayFromZero);

        // What's charged on top of the supplier's price.
        public static decimal Retained(decimal commission, decimal percent) =>
            commission <= 0m ? 0m : commission - Discount(commission, percent);
    }
}
