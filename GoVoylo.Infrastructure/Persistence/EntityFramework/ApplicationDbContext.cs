// GoVoylo.Infrastructure/Persistence/EntityFramework/ApplicationDbContext.cs
using GoVoylo.Domain.Entities;
using GoVoylo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace GoVoylo.Infrastructure.Persistence.EntityFramework;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<FlightBooking> FlightBookings => Set<FlightBooking>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<UserRegistration> UserRegistrations => Set<UserRegistration>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuthIdentity> AuthIdentities => Set<AuthIdentity>();
    public DbSet<Otp> OtpChallenges => Set<Otp>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerGstDetail> CustomerGstDetails => Set<CustomerGstDetail>();
    public DbSet<SavedTraveler> SavedTravelers => Set<SavedTraveler>();
    public DbSet<TravelerPassport> TravelerPassports => Set<TravelerPassport>();
    public DbSet<TravelerVisa> TravelerVisas => Set<TravelerVisa>();
    public DbSet<TravelerFrequentFlyer> TravelerFrequentFlyers => Set<TravelerFrequentFlyer>();
    public DbSet<TravelerSpecialAssistance> TravelerSpecialAssistances => Set<TravelerSpecialAssistance>();
    public DbSet<TravelerEmergencyContact> TravelerEmergencyContacts => Set<TravelerEmergencyContact>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
