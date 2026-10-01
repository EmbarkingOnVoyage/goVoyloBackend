namespace GoVoylo.Application.Common
{
    // Saved passports store the issuing country as the display name the mobile
    // app's own country picker offers (apps/Mobile/src/data/selectOptions.ts
    // COUNTRY_OPTIONS), while suppliers want ISO 3166-1 alpha-2 (Tripjack's pNat,
    // e.g. "IN"). An explicit table rather than RegionInfo, since .NET's English
    // region names don't match several of the picker's ("Czech Republic",
    // "Turkey", "Russia", "South Korea", ...).
    public static class CountryCodes
    {
        private static readonly Dictionary<string, string> NameToIso2 = new(StringComparer.OrdinalIgnoreCase)
        {
            ["India"] = "IN", ["Afghanistan"] = "AF", ["Albania"] = "AL", ["Algeria"] = "DZ",
            ["Argentina"] = "AR", ["Armenia"] = "AM", ["Australia"] = "AU", ["Austria"] = "AT",
            ["Azerbaijan"] = "AZ", ["Bahrain"] = "BH", ["Bangladesh"] = "BD", ["Belarus"] = "BY",
            ["Belgium"] = "BE", ["Bhutan"] = "BT", ["Bolivia"] = "BO", ["Bosnia and Herzegovina"] = "BA",
            ["Brazil"] = "BR", ["Brunei"] = "BN", ["Bulgaria"] = "BG", ["Cambodia"] = "KH",
            ["Canada"] = "CA", ["Chile"] = "CL", ["China"] = "CN", ["Colombia"] = "CO",
            ["Croatia"] = "HR", ["Cuba"] = "CU", ["Cyprus"] = "CY", ["Czech Republic"] = "CZ",
            ["Denmark"] = "DK", ["Ecuador"] = "EC", ["Egypt"] = "EG", ["Estonia"] = "EE",
            ["Ethiopia"] = "ET", ["Fiji"] = "FJ", ["Finland"] = "FI", ["France"] = "FR",
            ["Georgia"] = "GE", ["Germany"] = "DE", ["Ghana"] = "GH", ["Greece"] = "GR",
            ["Hong Kong"] = "HK", ["Hungary"] = "HU", ["Iceland"] = "IS", ["Indonesia"] = "ID",
            ["Iran"] = "IR", ["Iraq"] = "IQ", ["Ireland"] = "IE", ["Israel"] = "IL",
            ["Italy"] = "IT", ["Japan"] = "JP", ["Jordan"] = "JO", ["Kazakhstan"] = "KZ",
            ["Kenya"] = "KE", ["Kuwait"] = "KW", ["Kyrgyzstan"] = "KG", ["Laos"] = "LA",
            ["Latvia"] = "LV", ["Lebanon"] = "LB", ["Lithuania"] = "LT", ["Luxembourg"] = "LU",
            ["Malaysia"] = "MY", ["Maldives"] = "MV", ["Malta"] = "MT", ["Mauritius"] = "MU",
            ["Mexico"] = "MX", ["Moldova"] = "MD", ["Monaco"] = "MC", ["Mongolia"] = "MN",
            ["Montenegro"] = "ME", ["Morocco"] = "MA", ["Myanmar"] = "MM", ["Nepal"] = "NP",
            ["Netherlands"] = "NL", ["New Zealand"] = "NZ", ["Nigeria"] = "NG", ["North Korea"] = "KP",
            ["North Macedonia"] = "MK", ["Norway"] = "NO", ["Oman"] = "OM", ["Pakistan"] = "PK",
            ["Panama"] = "PA", ["Papua New Guinea"] = "PG", ["Paraguay"] = "PY", ["Peru"] = "PE",
            ["Philippines"] = "PH", ["Poland"] = "PL", ["Portugal"] = "PT", ["Qatar"] = "QA",
            ["Romania"] = "RO", ["Russia"] = "RU", ["Rwanda"] = "RW", ["Saudi Arabia"] = "SA",
            ["Serbia"] = "RS", ["Singapore"] = "SG", ["Slovakia"] = "SK", ["Slovenia"] = "SI",
            ["South Africa"] = "ZA", ["South Korea"] = "KR", ["Spain"] = "ES", ["Sri Lanka"] = "LK",
            ["Sweden"] = "SE", ["Switzerland"] = "CH", ["Taiwan"] = "TW", ["Tajikistan"] = "TJ",
            ["Tanzania"] = "TZ", ["Thailand"] = "TH", ["Tunisia"] = "TN", ["Turkey"] = "TR",
            ["Turkmenistan"] = "TM", ["Uganda"] = "UG", ["Ukraine"] = "UA", ["United Arab Emirates"] = "AE",
            ["United Kingdom"] = "GB", ["United States"] = "US", ["Uruguay"] = "UY", ["Uzbekistan"] = "UZ",
            ["Venezuela"] = "VE", ["Vietnam"] = "VN", ["Yemen"] = "YE", ["Zambia"] = "ZM",
            ["Zimbabwe"] = "ZW"
        };

        // Already-ISO values ("IN") pass through; an unknown name is returned
        // unchanged so the supplier's own validation reports it, rather than this
        // silently guessing.
        public static string ToIso2(string country)
        {
            var trimmed = country.Trim();
            if (trimmed.Length == 2)
            {
                return trimmed.ToUpperInvariant();
            }

            return NameToIso2.TryGetValue(trimmed, out var iso2) ? iso2 : trimmed;
        }
    }
}
