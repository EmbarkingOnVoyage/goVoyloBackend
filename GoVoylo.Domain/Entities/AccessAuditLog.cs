using GoVoylo.Domain.Common;

namespace GoVoylo.Domain.Entities
{
    // Where a sign-in or a payment attempt came from: the client's IP, where that
    // IP is, whether it looks safe (VPN / proxy / Tor / abuse), and for payments
    // how the customer paid. Written when the event happens; the IP lookup and
    // the payment mode are filled in afterwards by a background worker.
    public class AccessAuditLog : BaseEntity
    {
        public const string EventSignIn = "sign_in";
        public const string EventPayment = "payment";

        public const string LookupPending = "pending";
        public const string LookupDone = "done";
        public const string LookupPrivateIp = "private_ip";
        public const string LookupNotConfigured = "not_configured";
        public const string LookupFailed = "failed";

        public string EventType { get; private set; } = null!;
        public Guid? UserId { get; private set; }

        // sign_in: otp | password | google | facebook | apple
        public string? SignInMethod { get; private set; }

        // payment: one row per Razorpay order (each "Pay Now" attempt).
        public string? BookingReference { get; private set; }
        public string? GatewayOrderId { get; private set; }
        public string? GatewayPaymentId { get; private set; }
        public decimal? Amount { get; private set; }
        public string? Currency { get; private set; }
        public string? PaymentStatus { get; private set; } // Initiated | Succeeded | Failed
        public string? PaymentMethod { get; private set; } // card | upi | netbanking | wallet | emi | paylater
        public string? PaymentMethodDetail { get; private set; } // e.g. "Visa credit card, HDFC Bank"

        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        public string? City { get; private set; }
        public string? Region { get; private set; }
        public string? CountryCode { get; private set; }
        public string? Isp { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }

        // Residential | Wireless | Business | Hosting | VPN | TOR ... (proxycheck.io)
        public string? ConnectionType { get; private set; }
        public bool? IsVpn { get; private set; }
        public bool? IsProxy { get; private set; }
        public bool? IsTor { get; private set; }
        // A data-centre address rather than a home, office or mobile connection.
        public bool? IsHosting { get; private set; }
        // 0-100: 0-33 low, 34-66 high, 67-100 very high (proxycheck.io).
        public int? RiskScore { get; private set; }
        // False on a VPN, proxy or Tor or with a very high risk score; null until
        // the lookup has run.
        public bool? IsSecure { get; private set; }
        public string LookupStatus { get; private set; } = LookupPending;

        public DateTime? UpdatedAt { get; private set; }

        public static AccessAuditLog SignIn(Guid userId, string method, string? ipAddress, string? userAgent) => new()
        {
            EventType = EventSignIn,
            UserId = userId,
            SignInMethod = method,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        public static AccessAuditLog PaymentStarted(
            Guid? userId, string bookingReference, string gatewayOrderId, decimal amount, string currency,
            string? ipAddress, string? userAgent) => new()
        {
            EventType = EventPayment,
            UserId = userId,
            BookingReference = bookingReference,
            GatewayOrderId = gatewayOrderId,
            Amount = amount,
            Currency = currency,
            PaymentStatus = "Initiated",
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        public void CompletePayment(string status, string? gatewayPaymentId, string? method, string? methodDetail)
        {
            PaymentStatus = status;
            GatewayPaymentId = gatewayPaymentId ?? GatewayPaymentId;
            PaymentMethod = method ?? PaymentMethod;
            PaymentMethodDetail = methodDetail ?? PaymentMethodDetail;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLookup(
            string status, string? city = null, string? region = null, string? countryCode = null, string? isp = null,
            decimal? latitude = null, decimal? longitude = null, string? connectionType = null, bool? isVpn = null,
            bool? isProxy = null, bool? isTor = null, bool? isHosting = null, int? riskScore = null, bool? isSecure = null)
        {
            LookupStatus = status;
            City = city;
            Region = region;
            CountryCode = countryCode;
            Isp = isp;
            Latitude = latitude;
            Longitude = longitude;
            ConnectionType = connectionType;
            IsVpn = isVpn;
            IsProxy = isProxy;
            IsTor = isTor;
            IsHosting = isHosting;
            RiskScore = riskScore;
            IsSecure = isSecure;
            UpdatedAt = DateTime.UtcNow;
        }

        // Copies a finished lookup of the same IP instead of paying for another.
        public void CopyLookupFrom(AccessAuditLog other) => SetLookup(
            other.LookupStatus, other.City, other.Region, other.CountryCode, other.Isp, other.Latitude, other.Longitude,
            other.ConnectionType, other.IsVpn, other.IsProxy, other.IsTor, other.IsHosting, other.RiskScore, other.IsSecure);

        // Required by EF Core
        private AccessAuditLog()
        {
        }
    }
}
