namespace GoVoylo.Domain.Common
{
    // The 3 roles hardcoded into [Authorize(Roles = "...")] checks throughout the API.
    // Renaming or deleting one of these breaks authorization silently, so admin role
    // management must protect them specifically.
    public static class RoleNames
    {
        public const string Customer = "customer";
        public const string SupportAgent = "support_agent";
        public const string Superadmin = "superadmin";

        public static readonly IReadOnlySet<string> BuiltIn =
            new HashSet<string> { Customer, SupportAgent, Superadmin };
    }
}
