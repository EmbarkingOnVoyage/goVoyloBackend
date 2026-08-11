using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class OtpVerification
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string VerificationToken {get; set;} = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public  DateTime CreatedAt { get; set;}
        public bool isVerified { get; set; }

    }
}
