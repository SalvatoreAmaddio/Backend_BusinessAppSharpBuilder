using System.Runtime.InteropServices;
using System.Text;

namespace Backend.Utils
{
    /// <summary>
    /// This class holds a reference to default Credential Targets used within this Framework.
    /// </summary>
    public static class SysCredentailTargets
    {

        private static string _emailUserName = string.Empty;
        public static string EmailApp 
        { 
            get=> $"{_emailUserName}EMAIL_APP_CREDENTIAL";
            set => _emailUserName = value ;
        }

        public static string EmailAppEncrypterKey => $"{EmailApp}_Encrypter_Key";
        public static string EmailAppEncrypterIV => $"{EmailApp}_Encrypter_IV";

        public static readonly string UserLogin = $"{Sys.AppName}_USER_LOGIN_CREDENTIAL";
        public static readonly string UserLoginEncrypterKey = $"{UserLogin}_Encrypter_Key";
        public static readonly string UserLoginEncrypterIV = $"{UserLogin}_Encrypter_IV";
    }

    /// <summary>
    /// This class uses the Win32 API to store sensitive information in the Windows Credential Manager System. 
    /// The information gets encapsulated in a <see cref="Credential"/> object which is stored in the local computer.
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
        /// Replace the stored credential with a new one.
        /// </summary>
        /// <param name="cred">A <see cref="Credential"/> object</param>
        /// <returns>true if the credential was successfully replaced</returns>
        public static bool Replace(Credential cred) 
        {
            if (Exist(cred.Target)) 
                Delete(cred.Target);
            return Store(cred);
        }

        /// <summary>
        /// Stores credential's information in the Windows Credential Manager System.
        /// </summary>
        /// <param name="cred">A <see cref="Credential"/> object</param>
        /// <returns>true if the credential was successfully stored</returns>
        public static bool Store(Credential cred)
        {
            if (Exist(cred.Target)) throw new Exception("This credential is already stored.");
            byte[] byteArray = Encoding.Unicode.GetBytes(cred.Password); //Encode password.

            CREDENTIAL credential = new() //create Credential Struct
            {
                TargetName = cred.Target,
                UserName = cred.Username,
                CredentialBlob = Marshal.StringToCoTaskMemUni(cred.Password),
                CredentialBlobSize = (uint)byteArray.Length,
                Type = CRED_TYPE_GENERIC,
                Persist = 2  // STORE THE CREDENTIAL IN THE LOCL MACHINE
            };

            bool result = CredWrite(ref credential, 0);

            Marshal.FreeCoTaskMem(credential.CredentialBlob); //FREE MEMORY
            return result;
        }

        /// <summary>
        /// Check if the credential exists within the Windows Credential Manager System
        /// </summary>
        /// <param name="credential">A Credential object</param>
        /// <returns>true if it exists</returns>
        public static bool Exist(Credential credential) => Exist(credential.Target);

        /// <summary>
        /// Check if the credential exists within the Windows Credential Manager System
        /// </summary>
        /// <param name="target">Credential's Unique Identifier</param>
        /// <returns>true if it exists</returns>
        public static bool Exist(string target) => CredRead(target, CRED_TYPE_GENERIC, 0, out IntPtr credentialPtr);

        /// <summary>
        /// Retrieve the credential stored in the Windows Credential Manager System.
        /// </summary>
        /// <param name="target">a string that works as the credential unique identifier</param>
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
                    return new (target, username, password);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes the credential stored in the Windows Credential Manager System.
        /// </summary>
        /// <param name="target">a string that works as the credential unique identifier</param>
        /// <returns>true if the credential was successfully removed</returns>
        public static bool Delete(string target) => CredDelete(target, CRED_TYPE_GENERIC, 0);
    }

    /// <summary>
    /// Instantiates an object holding Credential's information.
    /// Credential objects are used in: <see cref="EmailSender"/>, <see cref="CurrentUser"/>
    /// </summary>
    /// <param name="target">The Credential Unique Identifier</param>
    /// <param name="username">The Name of the information to store.</param>
    /// <param name="password">The actual sensitive information to store.</param>
    public class Credential(string target, string username, string password) 
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        public string Target { get; private set; } = target;

        /// <summary>
        /// Name of the information to store.
        /// </summary>
        public string Username { get; private set; } = username;

        /// <summary>
        /// The actual sensitive information to store.
        /// </summary>
        public string Password { get; private set; } = password;

        public override bool Equals(object? obj)
        {
            return obj is Credential credential &&
                   Target == credential.Target;
        }

        public override int GetHashCode() => HashCode.Combine(Target);
    }
}
