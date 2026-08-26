namespace GoVoylo.Application.Interfaces
{
    // For sensitive at-rest fields (passport/visa numbers, frequent-flyer membership
    // numbers) — never store these as plaintext.
    public interface IEncryptionService
    {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] cipherBytes);
    }
}
