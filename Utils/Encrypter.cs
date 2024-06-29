using Backend.Exceptions;
using System.Security.Cryptography;

namespace Backend.Utils
{
    /// <summary>
    /// This class provides functionality to encrypt and decrypt strings.
    /// </summary>
    public class Encrypter
    {
        private byte[]? _secretKey;
        private byte[]? _initVector;
        private string Str { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Encrypter"/> class with a string to encrypt or decrypt.
        /// </summary>
        /// <param name="str">The string to encrypt or decrypt.</param>
        public Encrypter(string str)
        {
            Str = str;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Encrypter"/> class with a string to decrypt and the secret key and IV targets
        /// that are already stored on the local computer.
        /// <para/>
        /// If the targets are not found (i.e., no credentials are stored on the local computer), the constructor will throw a <see cref="CredentialFailure"/>.
        /// </summary>
        /// <param name="str">The string to encrypt or decrypt.</param>
        /// <param name="secret_key_Target">The target of the secret key which is stored on the local computer.</param>
        /// <param name="ivTarget">The target of the IV which is stored on the local computer.</param>
        /// <exception cref="CredentialFailure">Thrown when stored credentials cannot be read.</exception>
        public Encrypter(string str, string secret_key_Target, string ivTarget) : this(str)
        {
            bool read = ReadStoredKeyIV(secret_key_Target, ivTarget);
            if (!read) throw new CredentialFailure("Failed to read stored credentials!");
        }

        /// <summary>
        /// Reads the key and IV <see cref="Credential"/> objects stored on the local computer.
        /// </summary>
        /// <param name="secret_key_Target">The secret key's target.</param>
        /// <param name="ivTarget">The IV's target.</param>
        /// <returns>True if both targets could be read; otherwise, false.</returns>
        public bool ReadStoredKeyIV(string secret_key_Target, string ivTarget)
        {
            Credential? keyCredential = CredentialManager.Get(secret_key_Target);
            Credential? ivCredential = CredentialManager.Get(ivTarget);
            if (keyCredential != null && ivCredential != null)
            {
                _secretKey = Convert.FromBase64String(keyCredential.Password);
                _initVector = Convert.FromBase64String(ivCredential.Password);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stores the key and IV as <see cref="Credential"/> objects.
        /// <para/>
        /// <c>WARNING:</c> This method should be called after <see cref="Encrypt"/> or <see cref="Decrypt"/>.
        /// </summary>
        /// <param name="secret_key_Target">The secret key's target.</param>
        /// <param name="ivTarget">The IV's target.</param>
        /// <exception cref="InvalidTargetsException">Thrown when the targets are invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the key or IV is null.</exception>
        public void StoreKeyIV(string secret_key_Target, string ivTarget)
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (_secretKey != null && _initVector != null)
            {
                CredentialManager.Store(new(secret_key_Target, "key", Convert.ToBase64String(_secretKey)));
                CredentialManager.Store(new(ivTarget, "iv", Convert.ToBase64String(_initVector)));
                return;
            }
            throw new ArgumentNullException($"{_secretKey} and {_initVector} cannot be null. Call this method after you have either called {nameof(Encrypt)} or {nameof(Decrypt)}.");
        }

        /// <summary>
        /// Replaces the stored key and IV <see cref="Credential"/> objects with new ones.
        /// <para/>
        /// <c>WARNING:</c> This method should be called after <see cref="Encrypt"/> or <see cref="Decrypt"/>.
        /// </summary>
        /// <param name="secret_key_Target">The secret key's target.</param>
        /// <param name="ivTarget">The IV's target.</param>
        /// <exception cref="InvalidTargetsException">Thrown when the targets are invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the key or IV is null.</exception>
        public void ReplaceStoredKeyIV(string secret_key_Target, string ivTarget)
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (_secretKey == null && _initVector == null) throw new ArgumentNullException($"{_secretKey} and {_initVector} cannot be null. Call this method after you have either called {nameof(Encrypt)} or {nameof(Decrypt)}.");

            DeleteStoredKeyIV(secret_key_Target, ivTarget);
            StoreKeyIV(secret_key_Target, ivTarget);
        }

        /// <summary>
        /// Deletes the stored key and IV <see cref="Credential"/> objects.
        /// </summary>
        /// <param name="secret_key_Target">The secret key's target.</param>
        /// <param name="ivTarget">The IV's target.</param>
        /// <exception cref="InvalidTargetsException">Thrown when the targets are invalid.</exception>
        public void DeleteStoredKeyIV(string secret_key_Target, string ivTarget)
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (CredentialManager.Exist(secret_key_Target)) CredentialManager.Delete(secret_key_Target);
            if (CredentialManager.Exist(ivTarget)) CredentialManager.Delete(ivTarget);
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <returns>The encrypted string.</returns>
        public string Encrypt()
        {
            using (Aes aes = Aes.Create())
            {
                _secretKey ??= aes.Key;
                _initVector ??= aes.IV;
                return EncryptString(_secretKey, _initVector);
            }
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <returns>The original string value.</returns>
        public string Decrypt()
        {
            using (Aes aes = Aes.Create())
            {
                _secretKey ??= aes.Key;
                _initVector ??= aes.IV;
                return DecryptString(_secretKey, _initVector);
            }
        }

        private string DecryptString(byte[] key, byte[] iv)
        {
            byte[] buffer = Convert.FromBase64String(Str);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new(buffer))
                using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private string EncryptString(byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new())
                {
                    using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new(cs))
                    {
                        sw.Write(Str);
                    }

                    byte[] encrypted = ms.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }
    }

}