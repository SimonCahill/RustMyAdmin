using System;

namespace RustMyAdmin.Backend.Security {

    using System.Security.Cryptography;

    /// <summary>
    /// Provides a standard interface for encrypting data.
    /// </summary>
    public interface IEncryptor {

        /// <summary>
        /// Generates a unique salt, typically based off a UUID.
        /// </summary>
        /// <returns>A machine-/user-unique salt</returns>
        byte[] GenerateSalt();

        /// <summary>
        /// Encrypts a given string using the underlying RNG and salt.
        /// </summary>
        /// <param name="unencrpted">The unencrypted string to encrypt.</param>
        /// <returns>An array of bytes containing the encrypted data.</returns>
        byte[] Encrypt(string unencrpted);

        /// <summary>
        /// Encrypts a given string using the underlying RNG and salt.
        /// </summary>
        /// <param name="unencrpted">The unencrypted string to encrypt.</param>
        /// <returns>An array of bytes containing the encrypted data.</returns>
        Task<byte[]> EncryptAsync(string unencrpted);

        /// <summary>
        /// Encrypts a given array of bytes using the underlying RNG and salt.
        /// </summary>
        /// <param name="unencrypted">The unencrypted data to be encrpted.</param>
        /// <returns>An array of bytes containing the encrypted data,</returns>
        byte[] EncryptBytes(byte[] unencrypted);

        /// <summary>
        /// Encrypts a given array of bytes using the underlying RNG and salt.
        /// </summary>
        /// <param name="unencrypted">The unencrypted data to be encrpted.</param>
        /// <returns>An array of bytes containing the encrypted data,</returns>
        Task<byte[]> EncryptBytesAsync(byte[] unencrypted);

        /// <summary>
        /// Decrypts an encrypted string using the underlying RNG and salt.
        /// </summary>
        /// <param name="encrypted">The data to decrypt.</param>
        /// <returns>The decrypted data in string form</returns>
        string Decrypt(byte[] encrypted);

        /// <summary>
        /// Decrypts an encrypted string using the underlying RNG and salt.
        /// </summary>
        /// <param name="encrypted">The data to decrypt.</param>
        /// <returns>The decrypted data in string form</returns>
        Task<string> DecryptAsync(byte[] encrypted);

        /// <summary>
        /// Decrypts an encrypted array of bytes using the underlying RNG and salt.
        /// </summary>
        /// <param name="encrypted">The data to decrypt.</param>
        /// <returns>An array of bytes containing the decrypted data.</returns>
        byte[] DecryptBytes(byte[] encrypted);

        /// <summary>
        /// Decrypts an encrypted array of bytes using the underlying RNG and salt.
        /// </summary>
        /// <param name="encrypted">The data to decrypt.</param>
        /// <returns>An array of bytes containing the decrypted data.</returns>
        Task<byte[]> DecryptBytesAsync(byte[] encrypted);

    }
}

