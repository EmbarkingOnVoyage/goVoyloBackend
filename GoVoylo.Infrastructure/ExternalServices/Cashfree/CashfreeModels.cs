using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.ExternalServices.Cashfree
{
    public record CashfreeCreateOrderRequest(
    string order_id,
    decimal order_amount,
    string order_currency,
    CashfreeCustomerDetails customer_details
);

    public record CashfreeCustomerDetails(
        string customer_id,
        string customer_phone
    );

    public record CashfreeCreateOrderResponse(
        string order_id,
        decimal order_amount,
        string order_currency,
        string payment_session_id
    );
}
