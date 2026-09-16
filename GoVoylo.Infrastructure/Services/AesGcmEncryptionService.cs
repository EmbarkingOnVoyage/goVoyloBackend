using System.Security.Cryptography;
using System.Text;
using GoVoylo.Application.Interfaces;

namespace GoVoylo.Infrastructure.Services
{
    // Placeholder for the Azure Key Vault-backed key management named in the
    // architecture doc — reads a static key from the environment for now, same
    // maturity level as JWT_SECRET. Swap the key source here when Key Vault is wired up.
    public class AesGcmEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public AesGcmEncryptionService()
        {
            var keyBase64 = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
                ?? throw new InvalidOperationException("ENCRYPTION_KEY is not configured.");
            _key = Convert.FromBase64String(keyBase64);
        }

        public byte[] Encrypt(string plainText)
        {
            var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];

            using (var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            // Stored layout: nonce | tag | ciphertext
            return nonce.Concat(tag).Concat(cipherBytes).ToArray();
        }

        public string Decrypt(byte[] cipherBytes)
        {
            var nonceSize = AesGcm.NonceByteSizes.MaxSize;
            var tagSize = AesGcm.TagByteSizes.MaxSize;

            var nonce = cipherBytes[..nonceSize];
            var tag = cipherBytes[nonceSize..(nonceSize + tagSize)];
            var ciphertext = cipherBytes[(nonceSize + tagSize)..];
            var plainBytes = new byte[ciphertext.Length];

            using (var aesGcm = new AesGcm(_key, tagSize))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
