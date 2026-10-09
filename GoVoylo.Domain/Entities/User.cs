using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }

        public string? Email { get; private set; }

        public string? Phone { get; private set; }

        public string? PhoneCountryCode { get; private set; }

        public string? PasswordHash { get; private set; }

        public string FirstName { get; private set; } = null!;

        public string LastName { get; private set; } = null!;

        public bool IsEmailVerified { get; private set; }

        public bool IsPhoneVerified { get; private set; }

        public string? ProfileImageUrl { get; private set; }

        public string Status { get; private set; } = "active";

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public string? Gender { get; private set; }

        public DateTime? DateOfBirth { get; private set; }

        public string? Nationality { get; private set; }

        public string? MaritalStatus { get; private set; }

        public DateTime? Anniversary { get; private set; }

        public string? CityOfResidence { get; private set; }

        public string? State { get; private set; }

        public byte[]? PassportNumberEncrypted { get; private set; }

        public DateTime? PassportExpiryDate { get; private set; }

        public string? PassportIssuingCountry { get; private set; }

        public DateTime? PassportExpiryAlertSentAt { get; private set; }

        public byte[]? PanCardNumberEncrypted { get; private set; }

        public bool AutoAddTravelInsurance { get; private set; }

        public const string GuestStatus = "guest";
        public const string MergedGuestStatus = "merged";

        // Guest checkout: the email typed at checkout. It's kept out of Email (the
        // unique sign-in identity), so a guest record never claims an address;
        // signing in with this email later moves the guest's travellers and
        // bookings into that account (see GuestAccountMerger).
        public string? GuestEmail { get; private set; }

        public bool IsGuest => Status == GuestStatus;

        // Where booking emails go: the account email, or a guest's checkout email.
        public string? ContactEmail => Email ?? GuestEmail;


        public User(
        string email,
        string passwordHash,
        string? phone,
        string firstName,
        string lastName)
        {
            Id = Guid.NewGuid();

            Email = email;
            PasswordHash = passwordHash;
            Phone = phone;

            FirstName = firstName;
            LastName = lastName;

            IsEmailVerified = false;
            IsPhoneVerified = false;

            Status = "active";

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Passwordless signup: created the moment a brand-new email finishes its
        // first OTP verification, so email ownership is already proven and there's
        // no password to hash. First/last name are placeholders until the customer
        // fills in Personal Details.
        public User(string email, string firstName, string lastName)
        {
            Id = Guid.NewGuid();

            Email = email;
            PasswordHash = null;
            Phone = null;

            FirstName = firstName;
            LastName = lastName;

            IsEmailVerified = true;
            IsPhoneVerified = false;

            Status = "active";

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Required by EF Core
        private User()
        {
        }

        // One per guest checkout, so a guest session only ever sees what it
        // created itself — never another checkout's data under the same email.
        public static User CreateGuest(string email, string phone) => new User
        {
            Id = Guid.NewGuid(),
            GuestEmail = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            FirstName = "Guest",
            LastName = string.Empty,
            Status = GuestStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // The guest's travellers and bookings now belong to the signed-in account.
        public void MarkGuestMerged()
        {
            Status = MergedGuestStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string firstName, string lastName, string? phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }

        // The booking contact: sets the mobile when given; sets the email only when
        // the account has none (it's the sign-in identity, so an existing one never
        // changes here). A newly added email isn't verified.
        public void SetContactDetails(string? phone, string? email)
        {
            if (!string.IsNullOrWhiteSpace(phone))
            {
                Phone = phone;
            }
            // A guest's email stays in GuestEmail: Email is a real account's identity.
            if (Email == null && !IsGuest && !string.IsNullOrWhiteSpace(email))
            {
                Email = email.Trim();
                IsEmailVerified = false;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProfileImageUrl(string url)
        {
            ProfileImageUrl = url;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearProfileImageUrl()
        {
            ProfileImageUrl = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkDeleted()
        {
            Status = "deleted";
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend()
        {
            Status = "suspended";
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            Status = "active";
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateExtendedProfile(
            string? gender,
            DateTime? dateOfBirth,
            string? nationality,
            string? maritalStatus,
            DateTime? anniversary,
            string? cityOfResidence,
            string? state,
            byte[]? passportNumberEncrypted,
            DateTime? passportExpiryDate,
            string? passportIssuingCountry,
            byte[]? panCardNumberEncrypted,
            bool autoAddTravelInsurance)
        {
            if (passportExpiryDate != PassportExpiryDate)
            {
                // A changed expiry date (e.g. renewal) means any prior alert no
                // longer applies to the current date — allow re-alerting on it.
                PassportExpiryAlertSentAt = null;
            }

            Gender = gender;
            DateOfBirth = dateOfBirth;
            Nationality = nationality;
            MaritalStatus = maritalStatus;
            Anniversary = anniversary;
            CityOfResidence = cityOfResidence;
            State = state;
            PassportNumberEncrypted = passportNumberEncrypted;
            PassportExpiryDate = passportExpiryDate;
            PassportIssuingCountry = passportIssuingCountry;
            PanCardNumberEncrypted = panCardNumberEncrypted;
            AutoAddTravelInsurance = autoAddTravelInsurance;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkPassportExpiryAlertSent()
        {
            PassportExpiryAlertSentAt = DateTime.UtcNow;
        }
        public void ResetPassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }
    }
}
