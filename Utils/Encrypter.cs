using Backend.Exceptions;
using System.Security.Cryptography;

namespace Backend.Utils
{
    /// <summary>
    /// This class encrypts and decrypts strings.
    /// </summary>
    /// <param name="str">the string to encrypt or decrypt.</param>
    public class Encrypter(string str)
    {
        private byte[]? secret_key;
        private byte[]? initVector;
        private string Str { get; set; } = str;

        /// <summary>
        /// Initialise a Encrypter object with a string to decrypt and the SECRET KEY and IV targets 
        /// who are already stored in the local computer.<para/>
        /// If the targets are not found, (i.e. no credentials are stored in the local computer)  the constructor will throw a <see cref="CredentialFailure"/>.
        /// </summary>
        /// <param name="str">the string to encrypt or decrypt.</param>
        /// <param name="secret_key_Target">the target of the secret key which is stored in the local computer</param>
        /// <param name="ivTarget">the target of the IV which is stored in the local computer</param>
        /// <exception cref="CredentialFailure"></exception>
        public Encrypter(string str, string secret_key_Target, string ivTarget) : this(str) 
        {
            bool read = ReadStoredKeyIV(secret_key_Target, ivTarget);
            if (!read) throw new CredentialFailure("Failed to read stored credentials!");
        }

        /// <summary>
        /// It reads Key's and IV's <see cref="Credential"/> objects stored in the local computer.
        /// </summary>
        /// <param name="secret_key_Target">The Secret Key's Target</param>
        /// <param name="ivTarget">The IV's Target</param>
        /// <returns>true if both targets could be read; otherwise false.</returns>
        public bool ReadStoredKeyIV(string secret_key_Target, string ivTarget) 
        {
            Credential? keyCredential = CredentialManager.Get(secret_key_Target);
            Credential? ivCredential = CredentialManager.Get(ivTarget);
            if (keyCredential != null && ivCredential != null) 
            {
                secret_key = Convert.FromBase64String(keyCredential.Password);
                initVector = Convert.FromBase64String(ivCredential.Password);
                return true;
            }
            return false;
        }

        /// <summary>
        /// It stores a Key and IV as <see cref="Credential"/>'s objects.<para/>
        /// <c>WARNING:</c> 
        /// This method should be called after <see cref="Encrypt"/> or <see cref="Decrypt"/>
        /// </summary>
        /// <param name="secret_key_Target">The Secret Key's Target</param>
        /// <param name="ivTarget">The IV's Target</param>
        public void StoreKeyIV(string secret_key_Target, string ivTarget)
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (secret_key != null && initVector != null) 
            {
                CredentialManager.Store(new(secret_key_Target, "key", Convert.ToBase64String(secret_key)));
                CredentialManager.Store(new(ivTarget, "iv", Convert.ToBase64String(initVector)));
                return;
            }
            throw new ArgumentNullException($"{secret_key} and {initVector} cannot be null. Call this method after you have either called {nameof(Encrypt)} or {nameof(Decrypt)}");
        }

        /// <summary>
        /// It replaces the stored a Key and IV <see cref="Credential"/>'s objects with new ones.<para/>
        /// <c>WARNING:</c> 
        /// This method should be called after <see cref="Encrypt"/> or <see cref="Decrypt"/>
        /// </summary>
        /// <param name="secret_key_Target">The Secret Key's Target</param>
        /// <param name="ivTarget">The IV's Target</param>
        public void ReplaceStoredKeyIV(string secret_key_Target, string ivTarget)
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (secret_key == null && initVector == null) throw new ArgumentNullException($"{secret_key} and {initVector} cannot be null. Call this method after you have either called {nameof(Encrypt)} or {nameof(Decrypt)}");

            DeleteStoredKeyIV(secret_key_Target, ivTarget);
            StoreKeyIV(secret_key_Target, ivTarget);
        }


        /// <summary>
        /// It deletes the stored a Key and IV <see cref="Credential"/>'s objects.
        /// </summary>
        /// <param name="secret_key_Target">The Secret Key's Target</param>
        /// <param name="ivTarget">The IV's Target</param>
        public void DeleteStoredKeyIV(string secret_key_Target, string ivTarget) 
        {
            if (string.IsNullOrEmpty(secret_key_Target) && string.IsNullOrEmpty(ivTarget)) throw new InvalidTargetsException(secret_key_Target, ivTarget);
            if (CredentialManager.Exist(secret_key_Target)) CredentialManager.Delete(secret_key_Target);
            if (CredentialManager.Exist(ivTarget)) CredentialManager.Delete(ivTarget);
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <returns>The encrypted string</returns>
        public string Encrypt() 
        {
            using (Aes aes = Aes.Create())
            {
                secret_key ??= aes.Key;
                initVector ??= aes.IV;
                return EncryptString(secret_key, initVector);
            }
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <returns>The string with its original value</returns>
        public string Decrypt()
        {
            using (Aes aes = Aes.Create())
            {
                secret_key ??= aes.Key;
                initVector ??= aes.IV;
                return DecryptString(secret_key, initVector);
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