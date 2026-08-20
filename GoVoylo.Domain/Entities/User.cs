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

        public byte[]? PanCardNumberEncrypted { get; private set; }

        public bool AutoAddTravelInsurance { get; private set; }


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

        // Required by EF Core
        private User()
        {
        }

        public void UpdateProfile(string firstName, string lastName, string? phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
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
    }
}
