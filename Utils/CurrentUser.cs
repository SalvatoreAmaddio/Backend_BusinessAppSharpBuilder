using Backend.Database;
using Backend.Exceptions;
using Backend.Model;
using Backend.Enums;

namespace Backend.Utils
{
    /// <summary>
    /// Holds a static reference to the current user, wrapping methods and properties of the <see cref="IUser"/> interface for global use.
    /// </summary>
    public class CurrentUser
    {
        /// <summary>
        /// Gets the secret key target for encryption.
        /// </summary>
        public static string SecretKeyTarget => SysCredentailTargets.UserLoginEncrypterSecretKey;

        /// <summary>
        /// Gets the initialization vector (IV) target for encryption.
        /// </summary>
        public static string IVTarget => SysCredentailTargets.UserLoginEncrypterIV;

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public static IUser? Is { private get; set; }

        /// <summary>
        /// Gets the current user ID.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
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
        /// Gets or sets the current username.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
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
        /// Gets or sets the current user's password.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
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
        /// Gets or sets a flag indicating whether to save the user's credentials as a <see cref="Credential"/> object on the local computer.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
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
        /// Gets the number of attempts left for the current user.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        public static int Attempts
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.Attempts;
        }

        /// <summary>
        /// Gets the target for the current user to use in <see cref="CredentialManager"/> operations.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        public static string Target
        {
            get => (Is == null) ? throw new CurrentUserNotSetException() : Is.Target;
        }

        /// <summary>
        /// Retrieves the password from the database based on the username provided. 
        /// This password can be checked against another password provided by the user for login purposes.
        /// This method does not change the <see cref="Password"/> property.
        /// </summary>
        /// <param name="decrypt">True if the password should be decrypted; otherwise, false.</param>
        /// <returns>The user's password if found; otherwise, null.</returns>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        /// <exception cref="InvalidTargetsException">Thrown if the secret key or IV targets are invalid.</exception>
        public static string? FetchUserPassword(bool decrypt = false)
        {
            if (Is == null) throw new CurrentUserNotSetException();
            if (string.IsNullOrEmpty(SecretKeyTarget) || string.IsNullOrEmpty(IVTarget)) throw new InvalidTargetsException(SecretKeyTarget, IVTarget);
            List<QueryParameter> para = new() { new(nameof(UserName), Is.UserName) };
            IUser? user = DatabaseManager.Find("User")?.Retrieve(null, para).Cast<IUser>().FirstOrDefault();
            if (user == null) return null;
            UserID = user.UserID;

            if (decrypt)
            {
                try
                {
                    Encrypter encrypter = new(user.Password, SecretKeyTarget, IVTarget);
                    return encrypter.Decrypt();
                }
                catch (CredentialFailure ex)
                {
                    return ex.Message;
                }
            }
            return user.Password;
        }

        /// <summary>
        /// Changes the password for the current user. The new password will be encrypted.
        /// This method also removes the user's <see cref="Credential"/> object from the local computer,
        /// requiring the user to log in again on the next application startup.
        /// </summary>
        /// <param name="pwd">The new password.</param>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        /// <exception cref="InvalidTargetsException">Thrown if the secret key or IV targets are invalid.</exception>
        public static void ChangePassword(string pwd)
        {
            if (Is == null) throw new CurrentUserNotSetException();
            if (string.IsNullOrEmpty(SecretKeyTarget) || string.IsNullOrEmpty(IVTarget)) throw new InvalidTargetsException(SecretKeyTarget, IVTarget);
            Encrypter encrypter = new(pwd);
            Password = encrypter.Encrypt();
            List<QueryParameter> para = new() { new(nameof(Password), Is.Password), new(nameof(UserID), Is.UserID) };
            DatabaseManager.Find("User")?.Crud(CRUD.UPDATE, "UPDATE User SET Password=@Password WHERE UserID=@UserID", para);
            encrypter.ReplaceStoredKeyIV(SecretKeyTarget, IVTarget);
            Logout();
        }

        /// <summary>
        /// Attempts to log in.
        /// </summary>
        /// <param name="pwd">The password to check.</param>
        /// <returns>True if the login was successful; otherwise, false.</returns>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        public static bool Login(string? pwd)
        {
            if (Is == null) throw new CurrentUserNotSetException();
            return Is.Login(pwd);
        }

        /// <summary>
        /// Resets the <see cref="Attempts"/> property to its initial value.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        public static void ResetAttempts()
        {
            if (Is == null) throw new CurrentUserNotSetException();
            Is.ResetAttempts();
        }

        /// <summary>
        /// Attempts to get and read the user's <see cref="Credential"/> object.
        /// If found, sets the <see cref="UserName"/> and <see cref="Password"/> properties.
        /// </summary>
        /// <returns>True if the credential was found and read; otherwise, false.</returns>
        public static bool ReadCredential()
        {
            Credential? credential = CredentialManager.Get(Target);
            if (credential == null) return false;
            UserName = credential.Username;
            Password = credential.Password;
            return true;
        }

        /// <summary>
        /// Logs out and removes any user's <see cref="Credential"/> object stored on the local computer.
        /// </summary>
        /// <exception cref="CurrentUserNotSetException">Thrown if the current user is not set.</exception>
        public static void Logout()
        {
            if (Is == null) throw new CurrentUserNotSetException();
            Is.Logout();
        }
    }
}