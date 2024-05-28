using Backend.Database;
using Backend.Exceptions;
using Backend.Model;

namespace Backend.Utils
{
    /// <summary>
    /// This class is meant to hold a static reference to the current user. It wraps Methods and Properties of the <see cref="IUser"/> interface to be used globally.
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// Sets the Secret Key Target
        /// </summary>
        public static string SecretKeyTarget => SysCredentailTargets.UserLoginEncrypterKey;

        /// <summary>
        /// Sets the IVTarget.
        /// </summary>
        public static string IVTarget => SysCredentailTargets.UserLoginEncrypterIV;

        /// <summary>
        /// Sets the Current User.
        /// </summary>
        public static IUser? Is { private get; set; }

        /// <summary>
        /// Gets the current UserID.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static long UserID
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.UserID;
            private set
            {
                if (Is == null) throw new CurrentUserNotSetException();
                Is.UserID = value;
            }
        }

        /// <summary>
        /// Gets ans Sets the current UserName.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static string UserName
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.UserName;
            set
            {
                if (Is == null) throw new CurrentUserNotSetException();
                Is.UserName = value;
            }
        }

        /// <summary>
        /// Gets ans Sets the current User's Password.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static string Password
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.Password;
            set
            {
                if (Is == null) throw new CurrentUserNotSetException();
                Is.Password = value;
            }
        }

        /// <summary>
        /// Gets and Sets a flag to save User's access as a <see cref="Credential"/> object in the Local Computer.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static bool RememberMe
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.RememberMe;
            set
            {
                if (Is == null) throw new CurrentUserNotSetException();
                Is.RememberMe = value;
            }
        }

        /// <summary>
        /// Gets the number of Attempts left for the Current User.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static int Attempts
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.Attempts;
        }

        /// <summary>
        /// Gets the Target for the CurrentUser to deal with <see cref="CredentialManager"/> operations.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static string Target
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.Target;
        }

        /// <summary>
        /// Retrieves the password from the Database based on the username provided. 
        /// This password can be checked against another password provided by 
        /// the user for login's purposes.<para/>
        /// Sets the <paramref name="decrypt"/> argument to true to retrieve the password as decrypeted. The default is false.<para/>
        /// <c>DON'T GET CONFUSE:</c>
        /// This method does not change the <see cref="Password"/> property.
        /// </summary>
        /// <param name="decrypt">true if the Password should be decrypted</param>
        /// <returns>A password if the user record was found.</returns>
        /// <exception cref="CurrentUserNotSetException"></exception>
        /// <exception cref="InvalidTargetsException"></exception>
        public static string? FetchUserPassword(bool decrypt = false)
        {
            if (Is == null) throw new CurrentUserNotSetException();
            if (string.IsNullOrEmpty(SecretKeyTarget) && string.IsNullOrEmpty(IVTarget)) throw new InvalidTargetsException(SecretKeyTarget, IVTarget);
            List<QueryParameter> para = [new(nameof(UserName), Is.UserName)];
            IUser? user = DatabaseManager.Find("User")?.Retrieve(null, para).Cast<IUser>().FirstOrDefault();
            if (user == null) return null;
            UserID = user.UserID;

            if (decrypt) 
            {
                Encrypter encrypter = new(user.Password, SecretKeyTarget, IVTarget);
                return encrypter.Decrypt();
            }
            return user.Password;
        }

        /// <summary>
        /// It changes the password for the Current User. The new password will be encrypted. 
        /// This method also removes the User's <see cref="Credential"/> object from the local computer.
        /// Therefore the user will need to login again on the next Application's Startup.
        /// </summary>
        /// <param name="pwd">The new password</param>
        /// <exception cref="CurrentUserNotSetException"></exception>
        /// <exception cref="InvalidTargetsException"></exception>
        public static void ChangePassword(string pwd)
        {
            if (Is == null) throw new CurrentUserNotSetException();
            if (string.IsNullOrEmpty(SecretKeyTarget) && string.IsNullOrEmpty(IVTarget)) throw new InvalidTargetsException(SecretKeyTarget, IVTarget);
            Encrypter encrypter = new(pwd);
            Password = encrypter.Encrypt();
            List<QueryParameter> para = [new(nameof(Password), Is.Password), new(nameof(Is.UserID), Is.UserID)];
            DatabaseManager.Find("User")?.Crud(CRUD.UPDATE, $"UPDATE User SET Password=@Password WHERE UserID=@UserID", para);
            encrypter.ReplaceStoredKeyIV(SecretKeyTarget, IVTarget);
            Logout();
        }

        /// <summary>
        /// Attempts to login.
        /// </summary>
        /// <param name="pwd">The password to check</param>
        /// <returns>true if the login was successful</returns>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static bool Login(string? pwd) 
        {
            if (Is == null) throw new CurrentUserNotSetException();
            return Is.Login(pwd);
        }

        /// <summary>
        /// Resets the <see cref="Attempts"/> property to its initial value.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static void ResetAttempts()
        {
            if (Is == null) throw new CurrentUserNotSetException();
            Is.ResetAttempts();
        }

        /// <summary>
        /// Attempts to get and read the user's <see cref="Credential"/> object.
        /// If found, it sets the <see cref="UserName"/> and <see cref="Password"/> properties.
        /// </summary>
        /// <returns>true if the credential was found and read.</returns>
        public static bool ReadCredential()
        {
            Credential? credential = CredentialManager.Get(Target);
            if (credential == null) return false;
            UserName = credential.Username;
            Password = credential.Password;
            return true;
        }

        /// <summary>
        /// Logs out and remove any User's <see cref="Credential"/> object stored in the Local Computer.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException"></exception>
        public static void Logout()
        {
            if (Is == null) throw new CurrentUserNotSetException();
            Is.Logout();
        }
    }
}
