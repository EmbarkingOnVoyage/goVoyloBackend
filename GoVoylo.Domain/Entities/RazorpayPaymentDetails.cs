using GoVoylo.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class RazorpayPaymentDetails : BaseEntity
    {
        public Guid BookingPaymentId { get; private set; }

        public string RazorpaySignature { get; private set; }

        public RazorpayPaymentDetails(
            Guid bookingPaymentId,
            string razorpaySignature)
        {
            BookingPaymentId = bookingPaymentId;
            RazorpaySignature = razorpaySignature;
        }
    }
}
