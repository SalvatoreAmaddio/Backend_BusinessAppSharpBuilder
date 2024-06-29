using System.Runtime.InteropServices;
using System.Text;

namespace Backend.Utils
{
    /// <summary>
    /// This class holds a reference to default credential targets used within this framework.
    /// </summary>
    public static class SysCredentailTargets
    {
        private static string _emailUserName = string.Empty;

        /// <summary>
        /// Gets or sets the email application credential target.
        /// </summary>
        public static string EmailApp
        {
            get => $"{_emailUserName}EMAIL_APP_CREDENTIAL";
            set => _emailUserName = value;
        }

        /// <summary>
        /// Gets the email application encrypter secret key.
        /// </summary>
        public static string EmailAppEncrypterSecretKey => $"{EmailApp}_Encrypter_Key";

        /// <summary>
        /// Gets the email application encrypter initialization vector.
        /// </summary>
        public static string EmailAppEncrypterIV => $"{EmailApp}_Encrypter_IV";

        /// <summary>
        /// Gets the user login credential target.
        /// </summary>
        public static readonly string UserLogin = $"{Sys.AppName}_USER_LOGIN_CREDENTIAL";

        /// <summary>
        /// Gets the user login encrypter secret key.
        /// </summary>
        public static readonly string UserLoginEncrypterSecretKey = $"{UserLogin}_Encrypter_Key";

        /// <summary>
        /// Gets the user login encrypter initialization vector.
        /// </summary>
        public static readonly string UserLoginEncrypterIV = $"{UserLogin}_Encrypter_IV";
    }

    /// <summary>
    /// This class uses the Win32 API to store sensitive information in the Windows Credential Manager system. 
    /// The information is encapsulated in a <see cref="Credential"/> object which is stored on the local computer.
    /// </summary>
    public static class CredentialManager
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        private const uint CRED_TYPE_GENERIC = 1;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CredDelete(string target, uint type, uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void CredFree([In] IntPtr buffer);

        /// <summary>
        /// Replaces the stored credential with a new one.
        /// </summary>
        /// <param name="cred">A <see cref="Credential"/> object.</param>
        /// <returns>True if the credential was successfully replaced.</returns>
        public static bool Replace(Credential cred)
        {
            if (Exist(cred.Target))
                Delete(cred.Target);
            return Store(cred);
        }

        /// <summary>
        /// Stores credential information in the Windows Credential Manager system.
        /// </summary>
        /// <param name="cred">A <see cref="Credential"/> object.</param>
        /// <returns>True if the credential was successfully stored.</returns>
        /// <exception cref="Exception">Thrown if the credential is already stored.</exception>
        public static bool Store(Credential cred)
        {
            if (Exist(cred.Target)) throw new Exception("This credential is already stored.");
            byte[] byteArray = Encoding.Unicode.GetBytes(cred.Password); // Encode password.

            CREDENTIAL credential = new() // Create Credential Struct
            {
                TargetName = cred.Target,
                UserName = cred.Username,
                CredentialBlob = Marshal.StringToCoTaskMemUni(cred.Password),
                CredentialBlobSize = (uint)byteArray.Length,
                Type = CRED_TYPE_GENERIC,
                Persist = 2  // Store the credential in the local machine
            };

            bool result = CredWrite(ref credential, 0);

            Marshal.FreeCoTaskMem(credential.CredentialBlob); // Free memory
            return result;
        }

        /// <summary>
        /// Checks if the credential exists within the Windows Credential Manager system.
        /// </summary>
        /// <param name="credential">A <see cref="Credential"/> object.</param>
        /// <returns>True if it exists.</returns>
        public static bool Exist(Credential credential) => Exist(credential.Target);

        /// <summary>
        /// Checks if the credential exists within the Windows Credential Manager system.
        /// </summary>
        /// <param name="target">Credential's unique identifier.</param>
        /// <returns>True if it exists.</returns>
        public static bool Exist(string target) => CredRead(target, CRED_TYPE_GENERIC, 0, out IntPtr credentialPtr);

        /// <summary>
        /// Retrieves the credential stored in the Windows Credential Manager system.
        /// </summary>
        /// <param name="target">A string that works as the credential unique identifier.</param>
        /// <returns>A <see cref="Credential"/> object.</returns>
        public static Credential? Get(string target)
        {
            bool result = CredRead(target, CRED_TYPE_GENERIC, 0, out IntPtr credentialPtr);
            if (result)
            {
                object? pointer = Marshal.PtrToStructure(credentialPtr, typeof(CREDENTIAL));
                if (pointer == null) throw new Exception();
                CREDENTIAL credential = (CREDENTIAL)pointer;
                string password = Marshal.PtrToStringUni(credential.CredentialBlob, (int)(credential.CredentialBlobSize / 2));
                string username = credential.UserName;
                CredFree(credentialPtr);
                return new Credential(target, username, password);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes the credential stored in the Windows Credential Manager system.
        /// </summary>
        /// <param name="target">A string that works as the credential unique identifier.</param>
        /// <returns>True if the credential was successfully removed.</returns>
        public static bool Delete(string target) => CredDelete(target, CRED_TYPE_GENERIC, 0);
    }

    /// <summary>
    /// Instantiates an object holding credential information.
    /// Credential objects are used in: <see cref="EmailSender"/>, <see cref="CurrentUser"/>.
    /// </summary>
    public class Credential
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Credential"/> class.
        /// </summary>
        /// <param name="target">The credential unique identifier.</param>
        /// <param name="username">The name of the information to store.</param>
        /// <param name="password">The actual sensitive information to store.</param>
        public Credential(string target, string username, string password)
        {
            Target = target;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Gets the name of the information to store.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the actual sensitive information to store.
        /// </summary>
        public string Password { get; private set; }

        public override bool Equals(object? obj)
        {
            return obj is Credential credential && Target == credential.Target;
        }

        public override int GetHashCode() => HashCode.Combine(Target);
    }

}
