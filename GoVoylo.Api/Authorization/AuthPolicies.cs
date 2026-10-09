namespace GoVoylo.Api.Authorization
{
    public static class AuthPolicies
    {
        // A signed-in account — not a guest checkout token. Guests can book and
        // pay; the account itself (profile, preferences, addresses...) needs a
        // real sign-in.
        public const string RegisteredUser = "RegisteredUser";

        public const string GuestRole = "guest";
    }
}
