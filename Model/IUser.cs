
namespace Backend.Model
{
    /// <summary>
    /// This interface defines a set or properties and methods that a User class should implement.
    /// </summary>
    public interface IUser
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// If sets to true, the <see cref="Login(string?)"/> will call <see cref="SaveCredentials"/>.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets the number of attempts left to keep trying to login.
        /// </summary>
        public int Attempts { get; }

        /// <summary>
        /// Gets the Target to be used when storing <see cref="Utils.Credential"/> objects.
        /// A Target is a string which represents the Credential's Unique Identifier.
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// It attempts to login. If <see cref="RememberMe"/> is set to true, <see cref="SaveCredentials"/> is called.
        /// If the login is not successful, the <see cref="Attempts"/> property gets subtracted by 1.
        /// If <see cref="Attempts"/> got down to zero, this method should throw an Exception if called again.
        /// </summary>
        /// <param name="pwd">The password to be checked against</param>
        /// <returns>true if the login attempt was successful</returns>
        /// <exception cref="Exception"></exception>
        public bool Login(string? pwd);

        /// <summary>
        /// Deletes the <see cref="Utils.Credential"/> object what was stored by <see cref="SaveCredentials"/>.
        /// </summary>
        public void Logout();

        /// <summary>
        /// Stores a <see cref="Utils.Credential"/> object in the local computer. This method is called within the <see cref="Login(string?)"/> method if the <see cref="RememberMe"/> property is set to true.
        /// </summary>
        public void SaveCredentials();

        /// <summary>
        /// Reset the <see cref="Attempts"/> property to its original value.
        /// </summary>
        public void ResetAttempts();

    }
}
