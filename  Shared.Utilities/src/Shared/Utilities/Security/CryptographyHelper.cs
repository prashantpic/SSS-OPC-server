using System;
using System.Security.Cryptography;
using System.Text;

namespace Opc.System.Shared.Utilities.Security
{
    /// <summary>
    /// A utility class for performing common cryptographic tasks.
    /// Includes methods for creating SHA256 hashes and generating secure random data.
    /// </summary>
    public static class CryptographyHelper
    {
        /// <summary>
        /// Computes the SHA256 hash of a string.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>The lowercase hexadecimal representation of the SHA256 hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the input string is null.</exception>
        public static string ComputeSha256Hash(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Generates a cryptographically secure array of random bytes.
        /// </summary>
        /// <param name="byteCount">The number of random bytes to generate. Must be positive.</param>
        /// <returns>A byte array filled with cryptographically strong random values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if byteCount is not positive.</exception>
        public static byte[] GenerateSecureRandomBytes(int byteCount)
        {
            if (byteCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount), "Byte count must be a positive number.");
            }

            return RandomNumberGenerator.GetBytes(byteCount);
        }
    }
}