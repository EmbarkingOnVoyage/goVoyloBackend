namespace GoVoylo.Application.Common
{
    public static class MaskingHelper
    {
        // Never return a decrypted sensitive number to the client in full — only the
        // last 4 characters, enough for the customer to recognize which document it is.
        public static string MaskKeepLast4(string value)
        {
            if (value.Length <= 4)
            {
                return new string('•', value.Length);
            }

            return new string('•', value.Length - 4) + value[^4..];
        }
    }
}
